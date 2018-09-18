using System.Threading;
using System.Threading.Tasks;

namespace Framework.Domain.Events
{
    public interface IEventBus
    {
        Task Publish<T>(T @event, CancellationToken cancellationToken = default(CancellationToken)) where T : class, IEvent;
    }
}