using System.Threading;

namespace AppFactory.Framework.Domain.Events
{
    public interface IEventBusSyncWrapper
    {
        void Publish<T>(T @event, CancellationToken cancellationToken = default(CancellationToken)) where T : class, IEvent;
    }
}