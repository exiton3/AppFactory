using AppFactory.Framework.Logging;
using AppFactory.Framework.Messaging.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using AzureServiceBus = Azure.Messaging.ServiceBus;
using CoreMessage = AppFactory.Framework.Messaging.Abstractions.Message;

namespace AppFactory.Framework.Messaging.Azure.FunctionHandlers;

/// <summary>
/// Base class for Azure Functions isolated worker handlers processing Service Bus messages.
///
/// Uses the host DI container via IServiceScopeFactory — no separate ServiceProvider needed.
/// Register your IMessageHandler&lt;TMessage&gt; in HostBuilder.ConfigureServices and inject
/// IServiceScopeFactory + ILogger into the subclass constructor.
///
/// Usage:
/// - Queue:  [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnection")]
/// - Topic:  [ServiceBusTrigger("%TopicName%", "%SubscriptionName%", Connection = "ServiceBusConnection")]
/// </summary>
public abstract class ServiceBusFunctionHandlerBase<TMessage> where TMessage : CoreMessage, new()
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    protected ServiceBusFunctionHandlerBase(IServiceScopeFactory scopeFactory, ILogger logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(AzureServiceBus.ServiceBusReceivedMessage message, FunctionContext context)
        => await ExecuteAsync(message, context, "Queue");

    public async Task HandleTopicMessage(AzureServiceBus.ServiceBusReceivedMessage message, FunctionContext context)
        => await ExecuteAsync(message, context, $"Topic [Subject: {message.Subject}]");

    public async Task HandleBatch(AzureServiceBus.ServiceBusReceivedMessage[] messages, FunctionContext context)
    {
        _logger.AddTraceId(context.InvocationId);
        _logger.LogInfo("Service Bus batch of {Count} messages received", messages.Length);

        var failures = new List<Exception>();
        foreach (var message in messages)
        {
            try
            {
                await ProcessMessageAsync(message, context.CancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process message {MessageId} in batch", message.MessageId);
                failures.Add(e);
            }
        }

        if (failures.Count > 0)
            throw new AggregateException("One or more messages in the batch failed.", failures);
    }

    private async Task ExecuteAsync(AzureServiceBus.ServiceBusReceivedMessage message, FunctionContext context, string source)
    {
        _logger.AddTraceId(context.InvocationId);
        _logger.LogInfo("Service Bus {Source} message {MessageId} received", source, message.MessageId);
        try
        {
            await ProcessMessageAsync(message, context.CancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Execution failed for message {MessageId}", message.MessageId);
            throw;
        }
    }

    private async Task ProcessMessageAsync(AzureServiceBus.ServiceBusReceivedMessage serviceBusMessage, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Message {MessageId} received with Properties: {Properties}", serviceBusMessage.MessageId, GetAttributeLog(serviceBusMessage));

        await using var scope = _scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();

        _logger.LogTrace("Handler #{Hash} {HandlerName} started", handler.GetHashCode(), handler.GetType().Name);
        using (_logger.LogPerformance($"Handler #{handler.GetHashCode()} {handler.GetType().Name}"))
        {
            var message = MapMessage(serviceBusMessage);
            await handler.HandleAsync(message, cancellationToken);
        }
    }

    private static string GetAttributeLog(AzureServiceBus.ServiceBusReceivedMessage message)
    {
        var properties = message.ApplicationProperties
            .Select(x => $"{x.Key}={x.Value}")
            .ToList();

        properties.Add($"DeliveryCount={message.DeliveryCount}");
        properties.Add($"EnqueuedTime={message.EnqueuedTime}");
        properties.Add($"SequenceNumber={message.SequenceNumber}");

        if (!string.IsNullOrEmpty(message.CorrelationId))
            properties.Add($"CorrelationId={message.CorrelationId}");

        if (!string.IsNullOrEmpty(message.SessionId))
            properties.Add($"SessionId={message.SessionId}");

        return string.Join(", ", properties);
    }

    private static TMessage MapMessage(AzureServiceBus.ServiceBusReceivedMessage serviceBusMessage)
    {
        var attributes = new Dictionary<string, string>();

        foreach (var prop in serviceBusMessage.ApplicationProperties)
            attributes[prop.Key] = prop.Value?.ToString() ?? string.Empty;

        attributes["DeliveryCount"]    = serviceBusMessage.DeliveryCount.ToString();
        attributes["EnqueuedTimeUtc"]  = serviceBusMessage.EnqueuedTime.UtcDateTime.ToString("O");
        attributes["SequenceNumber"]   = serviceBusMessage.SequenceNumber.ToString();
        attributes["MessageId"]        = serviceBusMessage.MessageId;

        if (!string.IsNullOrEmpty(serviceBusMessage.CorrelationId))
            attributes["CorrelationId"] = serviceBusMessage.CorrelationId;

        if (!string.IsNullOrEmpty(serviceBusMessage.SessionId))
            attributes["SessionId"] = serviceBusMessage.SessionId;

        if (!string.IsNullOrEmpty(serviceBusMessage.Subject))
            attributes["Subject"] = serviceBusMessage.Subject;

        attributes["Source"] = serviceBusMessage.Subject ?? "ServiceBus";

        return new TMessage
        {
            Body             = serviceBusMessage.Body.ToString(),
            MessageId        = serviceBusMessage.MessageId,
            Properties       = attributes,
            EnqueuedTimeUtc  = serviceBusMessage.EnqueuedTime.UtcDateTime,
            DeliveryCount    = (int)serviceBusMessage.DeliveryCount
        };
    }
}
