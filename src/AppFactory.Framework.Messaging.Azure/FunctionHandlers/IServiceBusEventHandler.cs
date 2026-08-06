using AppFactory.Framework.Messaging.Abstractions;
using AzureServiceBus = Azure.Messaging.ServiceBus;

namespace AppFactory.Framework.Messaging.Azure.FunctionHandlers;

/// <summary>
/// Handles a single strongly-typed event deserialized from a Service Bus message.
/// Implement this per event type and register via AddServiceBusEventHandler.
/// Settlement (Complete / Abandon) is handled by ServiceBusEventRouterBase — do not call it here.
/// </summary>
public interface IServiceBusEventHandler<TEvent> where TEvent : CorrelatedEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
