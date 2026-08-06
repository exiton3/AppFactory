using AzureServiceBus = Azure.Messaging.ServiceBus;

namespace AppFactory.Framework.Messaging.Azure.FunctionHandlers;

/// <summary>
/// Binds a string EventType to a pre-compiled dispatch delegate so the router
/// never uses reflection at message-processing time.
/// </summary>
public sealed record EventHandlerRegistration(
    string EventType,
    Func<AzureServiceBus.ServiceBusReceivedMessage, IServiceProvider, CancellationToken, Task> Dispatch
);
