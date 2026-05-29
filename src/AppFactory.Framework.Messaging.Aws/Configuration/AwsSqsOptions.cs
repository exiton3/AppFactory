namespace AppFactory.Framework.Messaging.Aws.Configuration;

/// <summary>
/// Configuration options for AWS SQS messaging.
/// </summary>
public class AwsSqsOptions
{
    /// <summary>
    /// The SQS queue URL for publishing messages.
    /// </summary>
    public string QueueUrl { get; set; } = string.Empty;

    /// <summary>
    /// The dead letter queue URL for failed messages.
    /// </summary>
    public string? DeadLetterQueueUrl { get; set; }

    /// <summary>
    /// Maximum message delay in seconds (0-900). Default is 0.
    /// </summary>
    public int DelaySeconds { get; set; } = 0;

    /// <summary>
    /// Maximum number of retries for failed publish operations. Default is 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// AWS region for SQS. If not specified, uses default AWS SDK resolution.
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Enable detailed logging for message publishing. Default is false.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Maximum number of messages in a batch (1-10). Default is 10.
    /// </summary>
    public int MaxBatchSize { get; set; } = 10;
}
