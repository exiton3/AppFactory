using Azure.Messaging.ServiceBus;
using AppFactory.Framework.Logging.Abstractions;
using AppFactory.Framework.Messaging.Azure.Configuration;
using AppFactory.Framework.Messaging.Core.Abstractions;
using AppFactory.Framework.Shared;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AppFactory.Framework.Messaging.Azure;

/// <summary>
/// Azure Service Bus implementation of IMessagePublisher for queue-based messaging.
/// </summary>
public class ServiceBusMessagePublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly AzureServiceBusOptions _options;
    private readonly ILogger _logger;

    public ServiceBusMessagePublisher(
        ServiceBusClient client,
        IOptions<AzureServiceBusOptions> options,
        ILogger logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(_options.QueueName))
        {
            throw new ArgumentException("QueueName is required in AzureServiceBusOptions", nameof(options));
        }

        _sender = _client.CreateSender(_options.QueueName);
    }

    /// <summary>
    /// Publishes a single message to Azure Service Bus.
    /// </summary>
    public async Task<PublishResult> PublishAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken = default) where TMessage : class
    {
        Check.NotNull(message, nameof(message));

        try
        {
            var serviceBusMessage = CreateServiceBusMessage(message);

            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug($"Publishing message to Service Bus: {typeof(TMessage).Name}");
            }

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug($"Message published successfully. MessageId: {serviceBusMessage.MessageId}");
            }

            return PublishResult.Success(serviceBusMessage.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error publishing message to Service Bus: {ex.Message}");
            return PublishResult.Failed(ex.Message);
        }
    }

    /// <summary>
    /// Publishes a batch of messages to Azure Service Bus.
    /// </summary>
    public async Task<BatchPublishResult> PublishBatchAsync<TMessage>(
        IEnumerable<TMessage> messages,
        CancellationToken cancellationToken = default) where TMessage : class
    {
        Check.NotNull(messages, nameof(messages));

        var messageList = messages.ToList();
        if (messageList.Count == 0)
        {
            return BatchPublishResult.Success();
        }

        var results = new List<BatchPublishResult.MessageResult>();

        // Service Bus supports batching up to the configured max batch size
        var batches = messageList.Chunk(_options.MaxBatchSize);

        foreach (var batch in batches)
        {
            var batchResults = await PublishBatchInternalAsync(batch, cancellationToken);
            results.AddRange(batchResults);
        }

        var successCount = results.Count(r => r.IsSuccess);
        var failureCount = results.Count(r => !r.IsSuccess);

        _logger.LogInformation($"Batch publish completed. Success: {successCount}, Failed: {failureCount}");

        return new BatchPublishResult
        {
            IsSuccess = failureCount == 0,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results
        };
    }

    private async Task<List<BatchPublishResult.MessageResult>> PublishBatchInternalAsync<TMessage>(
        IEnumerable<TMessage> messages,
        CancellationToken cancellationToken) where TMessage : class
    {
        var results = new List<BatchPublishResult.MessageResult>();

        try
        {
            using var messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken);

            var messageList = messages.ToList();
            var addedMessages = new List<(string MessageId, int Index)>();

            for (int i = 0; i < messageList.Count; i++)
            {
                var serviceBusMessage = CreateServiceBusMessage(messageList[i]);

                if (messageBatch.TryAddMessage(serviceBusMessage))
                {
                    addedMessages.Add((serviceBusMessage.MessageId, i));
                }
                else
                {
                    // Message too large or batch full - send what we have
                    await _sender.SendMessagesAsync(messageBatch, cancellationToken);

                    // Mark sent messages as successful
                    foreach (var (msgId, _) in addedMessages)
                    {
                        results.Add(new BatchPublishResult.MessageResult
                        {
                            MessageId = msgId,
                            IsSuccess = true
                        });
                    }

                    // Start new batch with current message
                    messageBatch.Dispose();
                    var newBatch = await _sender.CreateMessageBatchAsync(cancellationToken);
                    
                    if (!newBatch.TryAddMessage(serviceBusMessage))
                    {
                        // Message is too large even for empty batch
                        results.Add(new BatchPublishResult.MessageResult
                        {
                            MessageId = serviceBusMessage.MessageId,
                            IsSuccess = false,
                            ErrorMessage = "Message size exceeds Service Bus limits"
                        });
                    }
                    else
                    {
                        addedMessages.Clear();
                        addedMessages.Add((serviceBusMessage.MessageId, i));
                    }
                }
            }

            // Send remaining messages
            if (addedMessages.Count > 0)
            {
                await _sender.SendMessagesAsync(messageBatch, cancellationToken);

                foreach (var (msgId, _) in addedMessages)
                {
                    results.Add(new BatchPublishResult.MessageResult
                    {
                        MessageId = msgId,
                        IsSuccess = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error publishing batch to Service Bus: {ex.Message}");

            // Mark all messages in this batch as failed
            foreach (var msg in messages)
            {
                results.Add(new BatchPublishResult.MessageResult
                {
                    MessageId = Guid.NewGuid().ToString(),
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        return results;
    }

    private ServiceBusMessage CreateServiceBusMessage<TMessage>(TMessage message) where TMessage : class
    {
        var messageBody = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var serviceBusMessage = new ServiceBusMessage(messageBody)
        {
            MessageId = Guid.NewGuid().ToString(),
            ContentType = "application/json"
        };

        // Add message type
        serviceBusMessage.ApplicationProperties["MessageType"] = typeof(TMessage).Name;

        // Add correlation tracking if message implements IMessage
        if (message is IMessage baseMessage)
        {
            serviceBusMessage.MessageId = baseMessage.MessageId;

            foreach (var prop in baseMessage.Properties)
            {
                serviceBusMessage.ApplicationProperties[prop.Key] = prop.Value;
            }

            // Service Bus native correlation support
            if (baseMessage.Properties.TryGetValue("CorrelationId", out var correlationId))
            {
                serviceBusMessage.CorrelationId = correlationId;
            }
        }

        // Set time to live if configured
        if (_options.TimeToLive.HasValue)
        {
            serviceBusMessage.TimeToLive = _options.TimeToLive.Value;
        }

        return serviceBusMessage;
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}
