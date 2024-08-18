using AppFactory.Framework.EventBus.EventBus.Events;

namespace AppFactory.Framework.EventBus.EventBus;

public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
    where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent @event);
}

public interface IIntegrationEventHandler
{
}
