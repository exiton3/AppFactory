namespace AppFactory.Framework.Messaging.Azure.Configuration;

/// <summary>
/// Configuration options for Azure Queue Storage messaging.
/// </summary>
public class AzureQueueStorageOptions
{
    /// <summary>
    /// The Storage Account connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The queue name for publishing messages.
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
    /// Message visibility timeout. Default is 30 seconds.
    /// </summary>
    public TimeSpan VisibilityTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Message time to live. If not specified, messages never expire.
    /// </summary>
    public TimeSpan? TimeToLive { get; set; }

    /// <summary>
    /// Maximum number of messages in a batch. Queue Storage doesn't support native batching,
    /// but we can send multiple messages sequentially. Default is 10 for performance.
    /// </summary>
    public int MaxBatchSize { get; set; } = 10;
}
