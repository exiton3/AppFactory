namespace AppFactory.Framework.Messaging.Abstractions;

/// <summary>
/// Platform-agnostic message publisher
/// Implementations: SQS (AWS), Service Bus (Azure), Queue Storage (Azure), RabbitMQ, Kafka
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publish a single message to the queue
    /// </summary>
    /// <typeparam name="TMessage">Message type</typeparam>
    /// <param name="message">Message to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Publish multiple messages in a batch (optimized for throughput)
    /// </summary>
    /// <typeparam name="TMessage">Message type</typeparam>
    /// <param name="messages">Messages to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishBatchAsync<TMessage>(IEnumerable<TMessage> messages, CancellationToken cancellationToken = default)
        where TMessage : class;
}

/// <summary>
/// Message publishing result
/// </summary>
public class PublishResult
{
    public bool IsSuccess { get; set; }
    public string MessageId { get; set; }
    public string Error { get; set; }

    public static PublishResult Success(string messageId) => new()
    {
        IsSuccess = true,
        MessageId = messageId
    };

    public static PublishResult Failure(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}

/// <summary>
/// Batch publishing result
/// </summary>
public class BatchPublishResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<PublishResult> Results { get; set; } = new();

    public bool AllSucceeded => FailureCount == 0;
    public bool AnyFailed => FailureCount > 0;
}
