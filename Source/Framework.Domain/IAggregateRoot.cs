using System.Collections.Generic;
using AppFactory.Framework.Domain.Events;

namespace AppFactory.Framework.Domain
{
    public interface IAggregateRoot
    {
        IEnumerable<IEvent> GetUncommittedChanges();
        void MarkChangesAsCommited();
    }
}