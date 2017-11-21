

using SuperNova.Shared.EventStore;
using SuperNova.Shared.EventStore.Transactions;
using SuperNova.Shared.Messaging;
using SuperNova.Shared.Supports;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNova.Storage.EventStore.Transactions
{
    public class ScopedTransaction : ITransaction
    {
        private IEventStream _stream;
        private Guid _aggregateId;
        private List<EventBase> _uncommittedEvents;
        private EventVersion _currentVersion;

        public ScopedTransaction(IEventStream stream, Guid aggregateId)
        {
            Ensure.ArgumentNotNull(stream, nameof(stream));            

            this._aggregateId = aggregateId;
            this._uncommittedEvents = new List<EventBase>();
            this._stream = stream;
        }

        public virtual async Task CreateAsync()
        {
            this._currentVersion = await this._stream.GetCurrentVersionAsync(this._aggregateId);
        }

        public void AddEvent(EventBase @event)
        {
            this._uncommittedEvents.Add(@event);
        }

        public void AddEvents(IEnumerable<EventBase> events)
        {
            this._uncommittedEvents.AddRange(events);
        }        

        public virtual async Task CommitAsync()
        {
            await this._stream.EmitEventsAsync(
                this._aggregateId, this._currentVersion,
                this._uncommittedEvents, CancellationToken.None);
        }
    }
}
