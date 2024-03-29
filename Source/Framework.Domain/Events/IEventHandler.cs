namespace AppFactory.Framework.Domain.Events
{
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        void Handle(TEvent eventObject);
    
    }
}