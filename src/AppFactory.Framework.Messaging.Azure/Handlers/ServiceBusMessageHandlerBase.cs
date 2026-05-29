using AppFactory.Framework.Logging.Abstractions;
using AppFactory.Framework.Messaging.Core.Abstractions;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;

namespace AppFactory.Framework.Messaging.Azure.Handlers;

/// <summary>
/// Base class for Azure Functions Service Bus message handlers with automatic deserialization.
/// </summary>
/// <typeparam name="TMessage">The message type to handle.</typeparam>
public abstract class ServiceBusMessageHandlerBase<TMessage> where TMessage : class
{
    protected readonly ILogger Logger;

    protected ServiceBusMessageHandlerBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Entry point for Azure Function. Processes Service Bus message and calls HandleMessageAsync.
    /// </summary>
    [Function(nameof(ServiceBusMessageHandlerBase<TMessage>))]
    public async Task Run(
        [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnection")] string messageBody,
        FunctionContext context)
    {
        try
        {
            var message = DeserializeMessage(messageBody);

            if (message == null)
            {
                Logger.LogError($"Failed to deserialize Service Bus message");
                throw new InvalidOperationException("Message deserialization failed");
            }

            // Populate metadata if message implements IMessage
            if (message is IMessage baseMessage && context.BindingContext.BindingData != null)
            {
                if (context.BindingContext.BindingData.TryGetValue("MessageId", out var msgId))
                {
                    baseMessage.MessageId = msgId?.ToString() ?? Guid.NewGuid().ToString();
                }

                if (context.BindingContext.BindingData.TryGetValue("DeliveryCount", out var deliveryCount))
                {
                    baseMessage.DeliveryCount = Convert.ToInt32(deliveryCount);
                }

                if (context.BindingContext.BindingData.TryGetValue("EnqueuedTimeUtc", out var enqueuedTime))
                {
                    baseMessage.EnqueuedTimeUtc = Convert.ToDateTime(enqueuedTime);
                }
            }

            Logger.LogInformation($"Handling Service Bus message: {typeof(TMessage).Name}");

            await HandleMessageAsync(message, CancellationToken.None);

            Logger.LogInformation($"Successfully processed Service Bus message");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error processing Service Bus message: {ex.Message}");
            throw; // Re-throw to move message to DLQ if configured
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
/// Base class for Azure Functions Service Bus message handlers with context support.
/// </summary>
/// <typeparam name="TMessage">The message type to handle.</typeparam>
public abstract class ServiceBusMessageHandlerWithContextBase<TMessage> where TMessage : class
{
    protected readonly ILogger Logger;

    protected ServiceBusMessageHandlerWithContextBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function(nameof(ServiceBusMessageHandlerWithContextBase<TMessage>))]
    public async Task Run(
        [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnection")] string messageBody,
        ServiceBusMessageActions messageActions,
        FunctionContext context)
    {
        var messageContext = new ServiceBusMessageContext(messageActions, Logger);

        try
        {
            var message = DeserializeMessage(messageBody);

            if (message == null)
            {
                Logger.LogError($"Failed to deserialize Service Bus message");
                await messageContext.DeadLetterAsync("Deserialization failed");
                return;
            }

            // Populate metadata
            if (message is IMessage baseMessage && context.BindingContext.BindingData != null)
            {
                if (context.BindingContext.BindingData.TryGetValue("MessageId", out var msgId))
                {
                    baseMessage.MessageId = msgId?.ToString() ?? Guid.NewGuid().ToString();
                }

                if (context.BindingContext.BindingData.TryGetValue("DeliveryCount", out var deliveryCount))
                {
                    baseMessage.DeliveryCount = Convert.ToInt32(deliveryCount);
                }

                if (context.BindingContext.BindingData.TryGetValue("EnqueuedTimeUtc", out var enqueuedTime))
                {
                    baseMessage.EnqueuedTimeUtc = Convert.ToDateTime(enqueuedTime);
                }
            }

            Logger.LogInformation($"Handling Service Bus message with context: {typeof(TMessage).Name}");

            await HandleMessageAsync(message, messageContext, CancellationToken.None);

            // If handler didn't explicitly complete or abandon, default to complete
            if (!messageContext.IsProcessed)
            {
                await messageContext.CompleteAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error processing Service Bus message: {ex.Message}");

            if (!messageContext.IsProcessed)
            {
                await messageContext.AbandonAsync(); // Return to queue for retry
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
/// Service Bus-specific implementation of IMessageContext for Azure Functions.
/// </summary>
internal class ServiceBusMessageContext : IMessageContext
{
    private readonly ServiceBusMessageActions _messageActions;
    private readonly ILogger _logger;
    public bool IsProcessed { get; private set; }

    public ServiceBusMessageContext(ServiceBusMessageActions messageActions, ILogger logger)
    {
        _messageActions = messageActions;
        _logger = logger;
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Message marked as completed");
        await _messageActions.CompleteMessageAsync(_messageActions.Message, cancellationToken);
        IsProcessed = true;
    }

    public async Task AbandonAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Message abandoned - will be retried");
        await _messageActions.AbandonMessageAsync(_messageActions.Message, cancellationToken: cancellationToken);
        IsProcessed = true;
    }

    public async Task DeadLetterAsync(string reason, CancellationToken cancellationToken = default)
    {
        _logger.LogError($"Message moved to DLQ. Reason: {reason}");
        await _messageActions.DeadLetterMessageAsync(
            _messageActions.Message,
            deadLetterReason: reason,
            cancellationToken: cancellationToken);
        IsProcessed = true;
    }
}
