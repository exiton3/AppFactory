using System.Collections.Generic;
using AppFactory.Framework.Domain.Events;

namespace AppFactory.Framework.Domain
{
    public abstract class AggregateRoot : Entity, IAggregateRoot
    {
        private readonly List<IEvent> _changes = new List<IEvent>();

        public IEnumerable<IEvent> GetUncommittedChanges()
        {
            lock (_changes)
            {
                return _changes.ToArray();
            }
        }

        public void MarkChangesAsCommited()
        {
            lock (_changes)
            {
                _changes.Clear();
            }
        }

        protected void Apply(IEvent @event)
        {
            lock (_changes)
            {
                _changes.Add(@event);
            }
        }
    }
}