using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using AppFactory.Framework.Logging.Abstractions;
using AppFactory.Framework.Messaging.Azure.Configuration;
using AppFactory.Framework.Messaging.Core.Abstractions;
using AppFactory.Framework.Shared;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace AppFactory.Framework.Messaging.Azure;

/// <summary>
/// Azure Queue Storage implementation of IMessagePublisher for simple queue-based messaging.
/// </summary>
public class QueueStorageMessagePublisher : IMessagePublisher
{
    private readonly QueueClient _queueClient;
    private readonly AzureQueueStorageOptions _options;
    private readonly ILogger _logger;

    public QueueStorageMessagePublisher(
        QueueClient queueClient,
        IOptions<AzureQueueStorageOptions> options,
        ILogger logger)
    {
        _queueClient = queueClient ?? throw new ArgumentNullException(nameof(queueClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Publishes a single message to Azure Queue Storage.
    /// </summary>
    public async Task<PublishResult> PublishAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken = default) where TMessage : class
    {
        Check.NotNull(message, nameof(message));

        try
        {
            var messageContent = SerializeMessage(message);

            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug($"Publishing message to Queue Storage: {typeof(TMessage).Name}");
            }

            var response = await _queueClient.SendMessageAsync(
                messageContent,
                visibilityTimeout: _options.VisibilityTimeout,
                timeToLive: _options.TimeToLive,
                cancellationToken: cancellationToken);

            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug($"Message published successfully. MessageId: {response.Value.MessageId}");
            }

            return PublishResult.Success(response.Value.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error publishing message to Queue Storage: {ex.Message}");
            return PublishResult.Failed(ex.Message);
        }
    }

    /// <summary>
    /// Publishes a batch of messages to Azure Queue Storage.
    /// Note: Queue Storage doesn't support native batching. Messages are sent sequentially.
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

        // Process in chunks for better performance
        var batches = messageList.Chunk(_options.MaxBatchSize);

        foreach (var batch in batches)
        {
            // Queue Storage doesn't support true batching, but we can send multiple messages in parallel
            var tasks = batch.Select(async msg =>
            {
                try
                {
                    var messageContent = SerializeMessage(msg);
                    var response = await _queueClient.SendMessageAsync(
                        messageContent,
                        visibilityTimeout: _options.VisibilityTimeout,
                        timeToLive: _options.TimeToLive,
                        cancellationToken: cancellationToken);

                    return new BatchPublishResult.MessageResult
                    {
                        MessageId = response.Value.MessageId,
                        IsSuccess = true
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error publishing message to Queue Storage: {ex.Message}");
                    return new BatchPublishResult.MessageResult
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        IsSuccess = false,
                        ErrorMessage = ex.Message
                    };
                }
            });

            var batchResults = await Task.WhenAll(tasks);
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

    private string SerializeMessage<TMessage>(TMessage message) where TMessage : class
    {
        // Create envelope with message type and correlation tracking
        var envelope = new QueueMessageEnvelope
        {
            MessageType = typeof(TMessage).Name,
            Payload = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            })
        };

        // Add correlation tracking if message implements IMessage
        if (message is IMessage baseMessage)
        {
            envelope.MessageId = baseMessage.MessageId;
            envelope.Properties = baseMessage.Properties;
        }

        var json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Queue Storage requires base64 encoding
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    /// <summary>
    /// Envelope for Queue Storage messages to include metadata.
    /// </summary>
    private class QueueMessageEnvelope
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public string MessageType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public Dictionary<string, string> Properties { get; set; } = new();
    }
}
