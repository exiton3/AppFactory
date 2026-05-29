using Amazon.SQS;
using Amazon.SQS.Model;
using AppFactory.Framework.Logging.Abstractions;
using AppFactory.Framework.Messaging.Aws.Configuration;
using AppFactory.Framework.Messaging.Core.Abstractions;
using AppFactory.Framework.Shared;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AppFactory.Framework.Messaging.Aws;

/// <summary>
/// AWS SQS implementation of IMessagePublisher for queue-based messaging.
/// </summary>
public class SqsMessagePublisher : IMessagePublisher
{
    private readonly IAmazonSQS _sqsClient;
    private readonly AwsSqsOptions _options;
    private readonly ILogger _logger;

    public SqsMessagePublisher(
        IAmazonSQS sqsClient,
        IOptions<AwsSqsOptions> options,
        ILogger logger)
    {
        _sqsClient = sqsClient ?? throw new ArgumentNullException(nameof(sqsClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(_options.QueueUrl))
        {
            throw new ArgumentException("QueueUrl is required in AwsSqsOptions", nameof(options));
        }
    }

    /// <summary>
    /// Publishes a single message to AWS SQS.
    /// </summary>
    public async Task<PublishResult> PublishAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken = default) where TMessage : class
    {
        Check.NotNull(message, nameof(message));

        try
        {
            var messageBody = SerializeMessage(message);
            var messageAttributes = BuildMessageAttributes(message);

            var request = new SendMessageRequest
            {
                QueueUrl = _options.QueueUrl,
                MessageBody = messageBody,
                MessageAttributes = messageAttributes,
                DelaySeconds = _options.DelaySeconds
            };

            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug($"Publishing message to SQS: {typeof(TMessage).Name}");
            }

            var response = await _sqsClient.SendMessageAsync(request, cancellationToken);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = $"Failed to publish message to SQS. Status: {response.HttpStatusCode}";
                _logger.LogError(error);
                return PublishResult.Failed(error);
            }

            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug($"Message published successfully. MessageId: {response.MessageId}");
            }

            return PublishResult.Success(response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error publishing message to SQS: {ex.Message}");
            return PublishResult.Failed(ex.Message);
        }
    }

    /// <summary>
    /// Publishes a batch of messages to AWS SQS.
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
            var entries = messages.Select((msg, index) => new SendMessageBatchRequestEntry
            {
                Id = index.ToString(),
                MessageBody = SerializeMessage(msg),
                MessageAttributes = BuildMessageAttributes(msg),
                DelaySeconds = _options.DelaySeconds
            }).ToList();

            var request = new SendMessageBatchRequest
            {
                QueueUrl = _options.QueueUrl,
                Entries = entries
            };

            var response = await _sqsClient.SendMessageBatchAsync(request, cancellationToken);

            // Process successful messages
            foreach (var success in response.Successful)
            {
                results.Add(new BatchPublishResult.MessageResult
                {
                    MessageId = success.MessageId,
                    IsSuccess = true
                });
            }

            // Process failed messages
            foreach (var failure in response.Failed)
            {
                results.Add(new BatchPublishResult.MessageResult
                {
                    MessageId = failure.Id,
                    IsSuccess = false,
                    ErrorMessage = $"Code: {failure.Code}, Message: {failure.Message}"
                });

                _logger.LogError($"Failed to publish message {failure.Id}: {failure.Code} - {failure.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error publishing batch to SQS: {ex.Message}");

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

    private string SerializeMessage<TMessage>(TMessage message) where TMessage : class
    {
        return JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
    }

    private Dictionary<string, MessageAttributeValue> BuildMessageAttributes<TMessage>(TMessage message) where TMessage : class
    {
        var attributes = new Dictionary<string, MessageAttributeValue>
        {
            ["MessageType"] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = typeof(TMessage).Name
            }
        };

        // Add correlation tracking if message implements IMessage
        if (message is IMessage baseMessage)
        {
            attributes["MessageId"] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = baseMessage.MessageId
            };

            foreach (var prop in baseMessage.Properties)
            {
                attributes[prop.Key] = new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = prop.Value
                };
            }
        }

        return attributes;
    }
}
