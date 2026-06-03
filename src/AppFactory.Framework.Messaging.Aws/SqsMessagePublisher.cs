using Amazon.SQS;
using Amazon.SQS.Model;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Messaging.Aws.Configuration;
using System.Text.Json;
using AppFactory.Framework.Messaging.Abstractions;
using Microsoft.Extensions.Options;

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
        ArgumentNullException.ThrowIfNull(message);

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
                var error = $"Failed to publish message to SQS. Status: {response}";
                _logger.LogError(error, response.HttpStatusCode);
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
        ArgumentNullException.ThrowIfNull(messages);

        var messageList = messages.ToList();
        if (messageList.Count == 0)
        {
            return BatchPublishResult.Success();
        }

        var allResults = new List<PublishResult>();
        var batches = messageList.Chunk(_options.MaxBatchSize);

        foreach (var batch in batches)
        {
            var batchResults = await PublishBatchInternalAsync(batch, cancellationToken);
            allResults.AddRange(batchResults);
        }

        var successCount = allResults.Count(r => r.IsSuccess);
        var failureCount = allResults.Count(r => !r.IsSuccess);

        _logger.LogInfo($"Batch publish completed. Success: {successCount}, Failed: {failureCount}");

        return new BatchPublishResult
        {
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = allResults
        };
    }

    private async Task<List<PublishResult>> PublishBatchInternalAsync<TMessage>(
        IEnumerable<TMessage> messages,
        CancellationToken cancellationToken) where TMessage : class
    {
        var results = new List<PublishResult>();

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
                results.Add(PublishResult.Success(success.MessageId));
            }

            // Process failed messages
            foreach (var failure in response.Failed)
            {
                var error = $"Code: {failure.Code}, Message: {failure.Message}";
                results.Add(PublishResult.Failed(error));

                _logger.LogError(
                    new Exception(error),
                    $"Failed to publish message {failure.Id}: {failure.Code} - {failure.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error publishing batch to SQS: {ex.Message}");

            // Mark all messages in this batch as failed
            foreach (var msg in messages)
            {
                results.Add(PublishResult.Failed(ex.Message));
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
