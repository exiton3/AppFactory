using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Messaging.Abstractions;
using System.Text.Json;


namespace AppFactory.Framework.Messaging.Aws.Handlers;

/// <summary>
/// Base class for AWS Lambda SQS message handlers with automatic deserialization.
/// </summary>
/// <typeparam name="TMessage">The message type to handle.</typeparam>
public abstract class LambdaMessageHandlerBase<TMessage> where TMessage : class
{
    protected readonly ILogger Logger;

    protected LambdaMessageHandlerBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Entry point for Lambda function. Processes SQS event and calls HandleMessageAsync for each message.
    /// </summary>
    
    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        if (sqsEvent?.Records == null || sqsEvent.Records.Count == 0)
        {
            Logger.LogInfo("Received empty SQS event");
            return;
        }

        Logger.LogInfo($"Processing {sqsEvent.Records.Count} SQS messages");

        var tasks = sqsEvent.Records.Select(record => ProcessRecordAsync(record, context));
        await Task.WhenAll(tasks);

        Logger.LogInfo("Completed processing all SQS messages");
    }

    private async Task ProcessRecordAsync(SQSEvent.SQSMessage record, ILambdaContext context)
    {
        try
        {
            var message = DeserializeMessage(record.Body);

            if (message == null)
            {
                Logger.LogError(new Exception("Deserialization failed"), $"Failed to deserialize message. MessageId: {record.MessageId}");
                throw new InvalidOperationException($"Message deserialization failed for {record.MessageId}");
            }

            // Populate metadata if message implements IMessage
            if (message is Message baseMessage)
            {
                baseMessage.MessageId = record.MessageId;
                baseMessage.EnqueuedTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(
                    long.Parse(record.Attributes["SentTimestamp"])).UtcDateTime;
                baseMessage.DeliveryCount = int.Parse(record.Attributes["ApproximateReceiveCount"]);

                // Extract correlation tracking from message attributes
                if (record.MessageAttributes != null)
                {
                    foreach (var attr in record.MessageAttributes)
                    {
                        baseMessage.Properties[attr.Key] = attr.Value.StringValue;
                    }
                }
            }

            Logger.LogInfo($"Handling message: {typeof(TMessage).Name}, MessageId: {record.MessageId}");

            await HandleMessageAsync(message, CancellationToken.None);

            Logger.LogInfo($"Successfully processed message: {record.MessageId}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error processing SQS message {record.MessageId}: {ex.Message}");
            throw; // Re-throw to move message to DLQ
        }
    }

    /// <summary>
    /// Override this method to implement message handling logic.
    /// </summary>
    protected abstract Task HandleMessageAsync(TMessage message, CancellationToken cancellationToken);

    private TMessage? DeserializeMessage(string messageBody)
    {
        try
        {
            return JsonSerializer.Deserialize<TMessage>(messageBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error deserializing message: {ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// Base class for AWS Lambda SQS message handlers with context support (Complete/Abandon/DeadLetter).
/// Note: SQS Lambda integration handles message deletion automatically on success.
/// Throwing exceptions will move message to DLQ if configured.
/// </summary>
/// <typeparam name="TMessage">The message type to handle.</typeparam>
public abstract class LambdaMessageHandlerWithContextBase<TMessage> where TMessage : class
{
    protected readonly ILogger Logger;

    protected LambdaMessageHandlerWithContextBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    
    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        if (sqsEvent?.Records == null || sqsEvent.Records.Count == 0)
        {
            Logger.LogInfo("Received empty SQS event");
            return;
        }

        Logger.LogInfo($"Processing {sqsEvent.Records.Count} SQS messages with context");

        var tasks = sqsEvent.Records.Select(record => ProcessRecordWithContextAsync(record, context));
        await Task.WhenAll(tasks);

        Logger.LogInfo("Completed processing all SQS messages");
    }

    private async Task ProcessRecordWithContextAsync(SQSEvent.SQSMessage record, ILambdaContext lambdaContext)
    {
        var messageContext = new SqsMessageContext(record, Logger);

        try
        {
            var message = DeserializeMessage(record.Body);

            if (message == null)
            {
                Logger.LogError(new Exception("Deserialization failed"), $"Failed to deserialize message. MessageId: {record.MessageId}");
                await messageContext.DeadLetterAsync("Deserialization failed");
                return;
            }

            // Populate metadata
            if (message is Message baseMessage)
            {
                baseMessage.MessageId = record.MessageId;
                baseMessage.EnqueuedTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(
                    long.Parse(record.Attributes["SentTimestamp"])).UtcDateTime;
                baseMessage.DeliveryCount = int.Parse(record.Attributes["ApproximateReceiveCount"]);

                if (record.MessageAttributes != null)
                {
                    foreach (var attr in record.MessageAttributes)
                    {
                        baseMessage.Properties[attr.Key] = attr.Value.StringValue;
                    }
                }
            }

            Logger.LogInfo($"Handling message with context: {typeof(TMessage).Name}, MessageId: {record.MessageId}");

            await HandleMessageAsync(message, messageContext, CancellationToken.None);

            // If handler didn't explicitly complete or abandon, default to complete
            if (!messageContext.IsProcessed)
            {
                await messageContext.CompleteAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error processing SQS message {record.MessageId}: {ex.Message}");

            if (!messageContext.IsProcessed)
            {
                await messageContext.AbandonAsync(); // Will throw to trigger retry
            }
        }
    }

    /// <summary>
    /// Override this method to implement message handling logic with context support.
    /// </summary>
    protected abstract Task HandleMessageAsync(
        TMessage message,
        IMessageContext context,
        CancellationToken cancellationToken);

    private TMessage? DeserializeMessage(string messageBody)
    {
        try
        {
            return JsonSerializer.Deserialize<TMessage>(messageBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error deserializing message: {ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// SQS-specific implementation of IMessageContext.
/// Note: In Lambda SQS integration, message visibility is managed automatically.
/// </summary>
internal class SqsMessageContext : IMessageContext
{
    private readonly SQSEvent.SQSMessage _record;
    private readonly ILogger _logger;
    public bool IsProcessed { get; private set; }

    public string MessageId => _record.MessageId;
    public string QueueName => _record.EventSourceArn?.Split(':').LastOrDefault()?.Split('/').LastOrDefault() ?? "unknown";
    public int DeliveryAttempt => int.Parse(_record.Attributes.GetValueOrDefault("ApproximateReceiveCount", "1"));
    public IDictionary<string, string> Properties { get; }
    public DateTime EnqueuedTimeUtc => DateTimeOffset.FromUnixTimeMilliseconds(
        long.Parse(_record.Attributes.GetValueOrDefault("SentTimestamp", "0"))).UtcDateTime;

    public SqsMessageContext(SQSEvent.SQSMessage record, ILogger logger)
    {
        _record = record;
        _logger = logger;

        // Extract properties from message attributes
        Properties = new Dictionary<string, string>();
        if (record.MessageAttributes != null)
        {
            foreach (var attr in record.MessageAttributes)
            {
                Properties[attr.Key] = attr.Value.StringValue ?? string.Empty;
            }
        }
    }

    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug($"Message {_record.MessageId} marked as completed");
        IsProcessed = true;
        // Lambda handles deletion automatically on success
        return Task.CompletedTask;
    }

    public Task AbandonAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInfo($"Message {_record.MessageId} abandoned - will be retried");
        IsProcessed = true;
        // Throw to make Lambda retry the message
        throw new MessageAbandonedException($"Message {_record.MessageId} was abandoned");
    }

    public Task DeadLetterAsync(string reason, CancellationToken cancellationToken = default)
    {
        _logger.LogError(new Exception(reason), $"Message {_record.MessageId} moved to DLQ. Reason: {reason}");
        IsProcessed = true;
        // Throw to move to DLQ
        throw new MessageDeadLetteredException($"Message {_record.MessageId} dead-lettered: {reason}");
    }
}

/// <summary>
/// Exception thrown when a message is explicitly abandoned.
/// </summary>
public class MessageAbandonedException : Exception
{
    public MessageAbandonedException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when a message is explicitly moved to dead letter queue.
/// </summary>
public class MessageDeadLetteredException : Exception
{
    public MessageDeadLetteredException(string message) : base(message) { }
}
