using System.Collections.Generic;
using Framework.Domain.Events;

namespace Framework.Domain
{
    public interface IAggregateRoot
    {
        IEnumerable<IEvent> GetUncommittedChanges();
        void MarkChangesAsCommited();
    }
}