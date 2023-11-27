namespace AppFactory.Framework.Domain.Events
{
    public interface IEventPublisher 
    {
        bool Publish<TEvent>(TEvent eventObject) where TEvent : IEvent;
    }
}