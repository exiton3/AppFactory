using AppFactory.Framework.EventBus.EventBus.Events;

namespace AppFactory.Framework.EventBus.EventBus;

public interface IEventBus
{
    void Publish<TEvent>(TEvent @event) where TEvent:IntegrationEvent;

    void Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;

    void Unsubscribe<T, TH>()
        where TH : IIntegrationEventHandler<T>
        where T : IntegrationEvent;
}