namespace AppFactory.Framework.EventBus.Abstractions;

/// <summary>
/// Platform-agnostic event handler
/// Process events from any source (EventBridge, Event Grid, RabbitMQ, etc.)
/// </summary>
public interface IEventHandler<TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Handle the event
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
