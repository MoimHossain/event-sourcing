
using System;
using System.Collections.Generic;
using SuperNova.Shared.Supports;
using SuperNova.Shared.Messaging;


namespace SuperNova.Shared.DomainObjects
{
    public abstract class AggregateRoot
    {
        private readonly List<EventBase> _changes = new List<EventBase>();
        
        public long Version { get; internal set; }

        public IEnumerable<EventBase> GetUncommittedChanges() => _changes;

        public void MarkChangesAsCommitted() => _changes.Clear();

        public void LoadsFromHistory(IEnumerable<EventBase> history)
        {
            foreach (var e in history) ApplyChange(e, false);
        }

        protected virtual void ApplyChange(EventBase @event) => ApplyChange(@event, true);


        // push atomic aggregate changes to local history for further processing (EventStore.SaveEvents)
        private void ApplyChange(EventBase @event, bool isNew)
        {
            this.AsDynamic().Apply(@event);
            if (isNew) _changes.Add(@event);
        }
    }
}
