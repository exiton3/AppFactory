using System.Threading;
using System.Threading.Tasks;

namespace Framework.Domain.Events
{
    public interface IEventHandlerAsync<in TEvent> : IEventHandlerAsync where TEvent : IEvent
    {
        Task Handle(TEvent message, CancellationToken token = default(CancellationToken));
    }

    public interface IEventHandlerAsync
    {
        Task Handle(object message, CancellationToken token = default(CancellationToken));
    }
}