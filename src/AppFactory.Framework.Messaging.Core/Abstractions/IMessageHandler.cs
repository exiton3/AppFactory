namespace AppFactory.Framework.Messaging.Abstractions;

/// <summary>
/// Platform-agnostic message handler
/// Business logic for processing messages from any queue
/// </summary>
/// <typeparam name="TMessage">Message type to handle</typeparam>
public interface IMessageHandler<TMessage> where TMessage : class
{
    /// <summary>
    /// Handle a message asynchronously
    /// </summary>
    /// <param name="message">The message to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Extended message handler with context
/// Provides access to message metadata and control operations (complete, abandon, dead-letter)
/// </summary>
/// <typeparam name="TMessage">Message type to handle</typeparam>
public interface IMessageHandler<TMessage, TContext> 
    where TMessage : class
    where TContext : IMessageContext
{
    /// <summary>
    /// Handle a message with full context
    /// </summary>
    /// <param name="message">The message to process</param>
    /// <param name="context">Message context with metadata and control operations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task HandleAsync(TMessage message, TContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Message context providing metadata and control operations
/// </summary>
public interface IMessageContext
{
    /// <summary>
    /// Unique message identifier
    /// </summary>
    string MessageId { get; }

    /// <summary>
    /// Queue or topic name
    /// </summary>
    string QueueName { get; }

    /// <summary>
    /// Number of delivery attempts
    /// </summary>
    int DeliveryAttempt { get; }

    /// <summary>
    /// Message properties/attributes
    /// </summary>
    IDictionary<string, string> Properties { get; }

    /// <summary>
    /// Time the message was enqueued
    /// </summary>
    DateTime EnqueuedTimeUtc { get; }

    /// <summary>
    /// Complete message processing (remove from queue)
    /// </summary>
    Task CompleteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Abandon message processing (return to queue for retry)
    /// </summary>
    Task AbandonAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Send message to dead letter queue
    /// </summary>
    /// <param name="reason">Reason for dead-lettering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeadLetterAsync(string reason, CancellationToken cancellationToken = default);
}
