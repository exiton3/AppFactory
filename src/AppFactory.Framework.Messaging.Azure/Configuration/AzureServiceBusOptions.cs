namespace AppFactory.Framework.Messaging.Azure.Configuration;

/// <summary>
/// Configuration options for Azure Service Bus messaging.
/// </summary>
public class AzureServiceBusOptions
{
    /// <summary>
    /// The Service Bus connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The Service Bus queue name for publishing messages.
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of retries for failed publish operations. Default is 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Enable detailed logging for message publishing. Default is false.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Maximum number of messages in a batch (1-100). Default is 100.
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;

    /// <summary>
    /// Message time to live. If not specified, uses Service Bus queue default.
    /// </summary>
    public TimeSpan? TimeToLive { get; set; }

    /// <summary>
    /// Enable sessions for FIFO message processing. Default is false.
    /// </summary>
    public bool EnableSessions { get; set; } = false;
}
