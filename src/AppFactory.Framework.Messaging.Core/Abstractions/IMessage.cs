namespace AppFactory.Framework.Messaging.Abstractions;

/// <summary>
/// Platform-agnostic message interface
/// Represents a message that can be published to any message queue (SQS, Service Bus, etc.)
/// </summary>
public interface IMessage
{
    /// <summary>
    /// Unique identifier for the message
    /// </summary>
    string MessageId { get; }

    /// <summary>
    /// Message body (typically JSON)
    /// </summary>
    string Body { get; set; }

    /// <summary>
    /// Message properties/attributes (metadata)
    /// </summary>
    IDictionary<string, string> Properties { get; }

    /// <summary>
    /// When the message was enqueued
    /// </summary>
    DateTime EnqueuedTimeUtc { get; }

    /// <summary>
    /// Number of times this message has been delivered (dequeue count)
    /// </summary>
    int DeliveryCount { get; }
}

/// <summary>
/// Base message implementation
/// </summary>
public class Message : IMessage
{
    public Message()
    {
        MessageId = Guid.NewGuid().ToString();
        Properties = new Dictionary<string, string>();
        EnqueuedTimeUtc = DateTime.UtcNow;
        DeliveryCount = 0;
    }

    public string MessageId { get; set; }
    public string Body { get; set; }
    public IDictionary<string, string> Properties { get; set; }
    public DateTime EnqueuedTimeUtc { get; set; }
    public int DeliveryCount { get; set; }

    /// <summary>
    /// Add correlation ID for distributed tracing
    /// </summary>
    public void AddCorrelationId(string correlationId)
    {
        Properties["CorrelationId"] = correlationId;
    }

    /// <summary>
    /// Add causation ID (ID of the command/event that caused this message)
    /// </summary>
    public void AddCausationId(string causationId)
    {
        Properties["CausationId"] = causationId;
    }

    /// <summary>
    /// Add user ID for audit trail
    /// </summary>
    public void AddUserId(string userId)
    {
        Properties["UserId"] = userId;
    }
}
