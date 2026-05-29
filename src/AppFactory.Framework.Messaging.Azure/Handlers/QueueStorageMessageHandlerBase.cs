using AppFactory.Framework.Logging.Abstractions;
using AppFactory.Framework.Messaging.Core.Abstractions;
using Microsoft.Azure.Functions.Worker;
using System.Text;
using System.Text.Json;

namespace AppFactory.Framework.Messaging.Azure.Handlers;

/// <summary>
/// Base class for Azure Functions Queue Storage message handlers with automatic deserialization.
/// </summary>
/// <typeparam name="TMessage">The message type to handle.</typeparam>
public abstract class QueueStorageMessageHandlerBase<TMessage> where TMessage : class
{
    protected readonly ILogger Logger;

    protected QueueStorageMessageHandlerBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Entry point for Azure Function. Processes Queue Storage message and calls HandleMessageAsync.
    /// </summary>
    [Function(nameof(QueueStorageMessageHandlerBase<TMessage>))]
    public async Task Run(
        [QueueTrigger("%QueueName%", Connection = "AzureWebJobsStorage")] string messageBody,
        FunctionContext context)
    {
        try
        {
            var envelope = DeserializeEnvelope(messageBody);

            if (envelope == null)
            {
                Logger.LogError("Failed to deserialize Queue Storage message envelope");
                throw new InvalidOperationException("Message envelope deserialization failed");
            }

            var message = JsonSerializer.Deserialize<TMessage>(envelope.Payload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (message == null)
            {
                Logger.LogError($"Failed to deserialize message payload");
                throw new InvalidOperationException("Message payload deserialization failed");
            }

            // Populate metadata if message implements IMessage
            if (message is IMessage baseMessage)
            {
                baseMessage.MessageId = envelope.MessageId;
                baseMessage.Properties = envelope.Properties ?? new Dictionary<string, string>();

                if (context.BindingContext.BindingData != null)
                {
                    if (context.BindingContext.BindingData.TryGetValue("DequeueCount", out var dequeueCount))
                    {
                        baseMessage.DeliveryCount = Convert.ToInt32(dequeueCount);
                    }

                    if (context.BindingContext.BindingData.TryGetValue("InsertionTime", out var insertionTime))
                    {
                        baseMessage.EnqueuedTimeUtc = Convert.ToDateTime(insertionTime);
                    }
                }
            }

            Logger.LogInformation($"Handling Queue Storage message: {typeof(TMessage).Name}");

            await HandleMessageAsync(message, CancellationToken.None);

            Logger.LogInformation("Successfully processed Queue Storage message");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error processing Queue Storage message: {ex.Message}");
            throw; // Re-throw to trigger poison message handling
        }
    }

    /// <summary>
    /// Override this method to implement message handling logic.
    /// </summary>
    protected abstract Task HandleMessageAsync(TMessage message, CancellationToken cancellationToken);

    private QueueMessageEnvelope? DeserializeEnvelope(string messageBody)
    {
        try
        {
            // Decode from base64
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(messageBody));

            return JsonSerializer.Deserialize<QueueMessageEnvelope>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error deserializing message envelope: {ex.Message}");
            return null;
        }
    }

    private class QueueMessageEnvelope
    {
        public string MessageId { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public Dictionary<string, string>? Properties { get; set; }
    }
}
