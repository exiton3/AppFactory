using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Serialization;
using AppFactory.Framework.Messaging.Abstractions;
using Azure.Storage.Queues.Models;
using CoreMessage = AppFactory.Framework.Messaging.Abstractions.Message;

namespace AppFactory.Framework.Messaging.Azure.FunctionHandlers;

/// <summary>
/// Base class for Azure Function handler to handle messages from Azure Storage Queue.
/// Supports cloud-agnostic IMessageHandler from Messaging.Core.
/// </summary>
/// <typeparam name="TMessage">message</typeparam>
public abstract class QueueStorageFunctionHandlerBase<TMessage> where TMessage : CoreMessage, new()
{
    protected ServiceProvider ServiceProvider;
    protected IJsonSerializer JsonSerializer;
    private IMessageHandler<TMessage> _handler;
    private ILogger _logger;
    private IStartup _startup;

    protected QueueStorageFunctionHandlerBase(IStartup startup = null)
    {
        _startup = startup;
        InitializeServices();
    }

    private void InitializeServices()
    {
        var services = new ServiceCollection();

        ConfigureServicesInt(services);
        ServiceProvider = services.BuildServiceProvider();
        JsonSerializer = ServiceProvider.GetRequiredService<IJsonSerializer>();
        _logger = ServiceProvider.GetRequiredService<ILogger>();
        _logger.LogInfo($"New instance of Queue Storage Function Handler {GetHashCode()} created");
    }

    private void ConfigureServicesInt(IServiceCollection services)
    {
        new DependencyModule().RegisterServices(services);

        _startup ??= GetStartup();
        _startup.ConfigureServices(services);
    }

    protected abstract IStartup GetStartup();

    /// <summary>
    /// Handle Queue Storage message
    /// Usage: [Function("FunctionName")]
    ///        public async Task Run([QueueTrigger("%QueueName%", Connection = "AzureWebJobsStorage")] QueueMessage queueMessage, FunctionContext context)
    ///        {
    ///            await Handle(queueMessage, context);
    ///        }
    /// </summary>
    public async Task Handle(QueueMessage queueMessage, FunctionContext context)
    {
        _logger.AddTraceId(context.InvocationId);

        try
        {
            await ProcessMessageAsync(queueMessage, context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"{e.Message} -- {e.StackTrace}");
            // Re-throw to trigger poison message handling
            throw;
        }
    }

    /// <summary>
    /// Handle Queue Storage message (string body)
    /// Usage: [Function("FunctionName")]
    ///        public async Task Run([QueueTrigger("%QueueName%", Connection = "AzureWebJobsStorage")] string messageBody, FunctionContext context)
    ///        {
    ///            await HandleString(messageBody, context);
    ///        }
    /// </summary>
    public async Task HandleString(string messageBody, FunctionContext context)
    {
        _logger.AddTraceId(context.InvocationId);

        try
        {
            await ProcessMessageAsync(messageBody, context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"{e.Message} -- {e.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Handle batch of Queue Storage messages
    /// Usage: [Function("FunctionName")]
    ///        public async Task Run([QueueTrigger("%QueueName%", Connection = "AzureWebJobsStorage")] QueueMessage[] messages, FunctionContext context)
    ///        {
    ///            await HandleBatch(messages, context);
    ///        }
    /// </summary>
    public async Task HandleBatch(QueueMessage[] queueMessages, FunctionContext context)
    {
        _logger.AddTraceId(context.InvocationId);
        _logger.LogInfo($"Records {queueMessages.Length} received");

        var errors = new List<Exception>();

        foreach (var message in queueMessages)
        {
            try
            {
                await ProcessMessageAsync(message, context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error processing message {message.MessageId}: {e.Message} -- {e.StackTrace}");
                errors.Add(e);
            }
        }

        if (errors.Any())
        {
            throw new AggregateException($"Failed to process {errors.Count} out of {queueMessages.Length} messages", errors);
        }
    }

    private async Task ProcessMessageAsync(QueueMessage queueMessage, FunctionContext context)
    {
        try
        {
            _logger.LogInfo($"Message {queueMessage.MessageId} received");
            _logger.LogTrace($"Message {queueMessage.MessageId} received, DequeueCount: {queueMessage.DequeueCount}");

            using var scope = ServiceProvider.CreateScope();
            _handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();

            _logger.LogTrace($"Queue Storage Message Handler #{_handler.GetHashCode()} {_handler.GetType().Name} started");

            using (_logger.LogPerformance($"Handler #{_handler.GetHashCode()} {_handler.GetType().Name}"))
            {
                var message = MapMessage(queueMessage);
                await _handler.HandleAsync(message, CancellationToken.None);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unhandled exception");
            throw;
        }
    }

    private async Task ProcessMessageAsync(string messageBody, FunctionContext context)
    {
        try
        {
            _logger.LogInfo($"Queue message received");

            using var scope = ServiceProvider.CreateScope();
            _handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();

            _logger.LogTrace($"Queue Storage Message Handler #{_handler.GetHashCode()} {_handler.GetType().Name} started");

            using (_logger.LogPerformance($"Handler #{_handler.GetHashCode()} {_handler.GetType().Name}"))
            {
                var message = MapMessage(messageBody);
                await _handler.HandleAsync(message, CancellationToken.None);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unhandled exception");
            throw;
        }
    }

    private TMessage MapMessage(QueueMessage queueMessage)
    {
        var message = new TMessage
        {
            Body = queueMessage.MessageText,
            MessageId = queueMessage.MessageId,
            Properties = new Dictionary<string, string>
            {
                ["Source"] = "QueueStorage",
                ["DequeueCount"] = queueMessage.DequeueCount.ToString(),
                ["InsertedOn"] = queueMessage.InsertedOn.HasValue ? queueMessage.InsertedOn.Value.UtcDateTime.ToString("O") : string.Empty,
                ["ExpiresOn"] = queueMessage.ExpiresOn.HasValue ? queueMessage.ExpiresOn.Value.UtcDateTime.ToString("O") : string.Empty,
                ["NextVisibleOn"] = queueMessage.NextVisibleOn.HasValue ? queueMessage.NextVisibleOn.Value.UtcDateTime.ToString("O") : string.Empty,
                ["PopReceipt"] = queueMessage.PopReceipt ?? string.Empty
            },
            EnqueuedTimeUtc = queueMessage.InsertedOn?.UtcDateTime ?? DateTime.UtcNow,
            DeliveryCount = (int)queueMessage.DequeueCount
        };

        return message;
    }

    private TMessage MapMessage(string messageBody)
    {
        var message = new TMessage
        {
            Body = messageBody,
            MessageId = Guid.NewGuid().ToString(),
            Properties = new Dictionary<string, string>
            {
                ["Source"] = "QueueStorage"
            },
            EnqueuedTimeUtc = DateTime.UtcNow,
            DeliveryCount = 1
        };

        return message;
    }
}
