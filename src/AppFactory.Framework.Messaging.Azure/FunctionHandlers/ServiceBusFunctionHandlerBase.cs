using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Messaging.Abstractions;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using AzureServiceBus = Azure.Messaging.ServiceBus;
using CoreMessage = AppFactory.Framework.Messaging.Abstractions.Message;

namespace AppFactory.Framework.Messaging.Azure.FunctionHandlers;

/// <summary>
/// Base class for Azure Function handlers processing Service Bus Queue and Topic messages.
/// Supports cloud-agnostic IMessageHandler from Messaging.Core.
/// 
/// Usage:
/// - For Queue: [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnection")]
/// - For Topic: [ServiceBusTrigger("%TopicName%", "%SubscriptionName%", Connection = "ServiceBusConnection")]
/// </summary>
/// <typeparam name="TMessage">The message type to process</typeparam>
public abstract class ServiceBusFunctionHandlerBase<TMessage> where TMessage : CoreMessage, new()
{
    protected ServiceProvider ServiceProvider;
    protected IJsonSerializer JsonSerializer;
    private IMessageHandler<TMessage> _handler;
    private ILogger _logger;
    private IStartup _startup;

    protected ServiceBusFunctionHandlerBase(IStartup startup = null)
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
        _logger.LogInfo($"New instance of ServiceBus Function Handler {GetHashCode()} created");
    }

    private void ConfigureServicesInt(IServiceCollection services)
    {
        new DependencyModule().RegisterServices(services);

        _startup ??= GetStartup();
        _startup.ConfigureServices(services);
    }

    protected abstract IStartup GetStartup();

    /// <summary>
    /// Handle a single Service Bus Queue message
    /// </summary>
    public async Task Handle(AzureServiceBus.ServiceBusReceivedMessage message, FunctionContext context)
    {
        _logger.AddTraceId(context.InvocationId);
        _logger.LogInfo($"Service Bus Queue message {message.MessageId} received");

        try
        {
            await ProcessMessageAsync(message, context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"{e.Message} -- {e.StackTrace}");
            throw; // Let Azure Functions handle retry logic
        }
    }

    /// <summary>
    /// Handle a single Service Bus Topic message
    /// </summary>
    public async Task HandleTopicMessage(AzureServiceBus.ServiceBusReceivedMessage message, FunctionContext context)
    {
        _logger.AddTraceId(context.InvocationId);
        _logger.LogInfo($"Service Bus Topic message {message.MessageId} received from subject: {message.Subject}");

        try
        {
            await ProcessMessageAsync(message, context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"{e.Message} -- {e.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Handle a batch of Service Bus messages
    /// </summary>
    public async Task HandleBatch(AzureServiceBus.ServiceBusReceivedMessage[] messages, FunctionContext context)
    {
        _logger.AddTraceId(context.InvocationId);
        _logger.LogInfo($"Service Bus batch of {messages.Length} messages received");

        foreach (var message in messages)
        {
            try
            {
                await ProcessMessageAsync(message, context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to process message {message.MessageId}: {e.Message}");
                // Continue processing other messages in the batch
            }
        }
    }

    private async Task ProcessMessageAsync(AzureServiceBus.ServiceBusReceivedMessage serviceBusMessage, FunctionContext context)
    {
        try
        {
            _logger.LogInfo($"Message {serviceBusMessage.MessageId} from Service Bus received");

            var attributes = GetLogMessageForAttributes(serviceBusMessage);
            _logger.LogTrace($"Message {serviceBusMessage.MessageId} received with Properties: {attributes}");

            using var scope = ServiceProvider.CreateScope();
            _handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();

            _logger.LogTrace($"Service Bus Message Handler #{_handler.GetHashCode()} {_handler.GetType().Name} started");
            using (_logger.LogPerformance($"Handler #{_handler.GetHashCode()} {_handler.GetType().Name}"))
            {
                var message = MapMessage(serviceBusMessage);
                await _handler.HandleAsync(message, CancellationToken.None);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unhandled exception");
            throw;
        }
    }

    private static string GetLogMessageForAttributes(AzureServiceBus.ServiceBusReceivedMessage serviceBusMessage)
    {
        var properties = serviceBusMessage.ApplicationProperties
            .Select(x => $"{x.Key}={x.Value}")
            .ToList();

        properties.Add($"DeliveryCount={serviceBusMessage.DeliveryCount}");
        properties.Add($"EnqueuedTime={serviceBusMessage.EnqueuedTime}");
        properties.Add($"SequenceNumber={serviceBusMessage.SequenceNumber}");

        if (!string.IsNullOrEmpty(serviceBusMessage.CorrelationId))
            properties.Add($"CorrelationId={serviceBusMessage.CorrelationId}");

        if (!string.IsNullOrEmpty(serviceBusMessage.SessionId))
            properties.Add($"SessionId={serviceBusMessage.SessionId}");

        return string.Join(", ", properties);
    }

    private TMessage MapMessage(AzureServiceBus.ServiceBusReceivedMessage serviceBusMessage)
    {
        var attributes = new Dictionary<string, string>();

        // Map application properties
        foreach (var prop in serviceBusMessage.ApplicationProperties)
        {
            attributes[prop.Key] = prop.Value?.ToString() ?? string.Empty;
        }

        // Map system properties
        attributes["DeliveryCount"] = serviceBusMessage.DeliveryCount.ToString();
        attributes["EnqueuedTimeUtc"] = serviceBusMessage.EnqueuedTime.UtcDateTime.ToString("O");
        attributes["SequenceNumber"] = serviceBusMessage.SequenceNumber.ToString();
        attributes["MessageId"] = serviceBusMessage.MessageId;

        if (!string.IsNullOrEmpty(serviceBusMessage.CorrelationId))
            attributes["CorrelationId"] = serviceBusMessage.CorrelationId;

        if (!string.IsNullOrEmpty(serviceBusMessage.SessionId))
            attributes["SessionId"] = serviceBusMessage.SessionId;

        if (!string.IsNullOrEmpty(serviceBusMessage.Subject))
            attributes["Subject"] = serviceBusMessage.Subject;

        attributes["Source"] = serviceBusMessage.Subject ?? "ServiceBus";

        var message = new TMessage
        {
            Body = serviceBusMessage.Body.ToString(),
            MessageId = serviceBusMessage.MessageId,
            Properties = attributes,
            EnqueuedTimeUtc = serviceBusMessage.EnqueuedTime.UtcDateTime,
            DeliveryCount = (int)serviceBusMessage.DeliveryCount
        };

        return message;
    }
}
