using AppFactory.Framework.Logging;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Messaging.Azure.FunctionHandlers;

/// <summary>
/// Base class for single-subscription / multi-event-type Service Bus functions.
/// Reads ApplicationProperties["EventType"], dispatches to the registered
/// IServiceBusEventHandler for that type, and handles Complete / Abandon settlement.
///
/// Usage:
///   1. Extend this class and expose the Azure Functions trigger method.
///   2. Call RouteAsync(message, messageActions, cancellationToken) from that method.
///   3. Register handlers: services.AddServiceBusEventHandler{TEvent, THandler}("EventTypeName")
/// </summary>
public abstract class ServiceBusEventRouterBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;
    private readonly IReadOnlyDictionary<string, EventHandlerRegistration> _handlers;

    protected ServiceBusEventRouterBase(
        IServiceScopeFactory scopeFactory,
        ILogger logger,
        IEnumerable<EventHandlerRegistration> registrations)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger       = logger       ?? throw new ArgumentNullException(nameof(logger));
        _handlers     = registrations.ToDictionary(r => r.EventType, StringComparer.Ordinal);
    }

    protected async Task RouteAsync(
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions actions,
        CancellationToken cancellationToken)
    {
        if (!message.ApplicationProperties.TryGetValue("EventType", out var et) || et is null)
        {
            _logger.LogError("Message {MessageId} is missing EventType — dead-lettering", message.MessageId);
            await actions.DeadLetterMessageAsync(message, deadLetterReason: "MissingEventType", cancellationToken: cancellationToken);
            return;
        }

        var eventType = et.ToString()!;

        if (!_handlers.TryGetValue(eventType, out var registration))
        {
            _logger.LogError("No handler registered for EventType {EventType} on message {MessageId} — dead-lettering", eventType, message.MessageId);
            await actions.DeadLetterMessageAsync(message, deadLetterReason: $"UnknownEventType:{eventType}", cancellationToken: cancellationToken);
            return;
        }

        try
        {
            _logger.LogInfo("Routing {EventType} message {MessageId}", eventType, message.MessageId);

            await using var scope = _scopeFactory.CreateAsyncScope();
            await registration.Dispatch(message, scope.ServiceProvider, cancellationToken);

            await actions.CompleteMessageAsync(message, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Handler failed for {EventType} message {MessageId}", eventType, message.MessageId);
            await actions.AbandonMessageAsync(message, cancellationToken: cancellationToken);
        }
    }
}
