using SuperNova.Shared.Messaging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNova.Shared.EventStore
{
    public interface IEventStream
    {
        Task<EventVersion> GetCurrentVersionAsync(Guid aggregateId);

        Task EmitEventsAsync(
            Guid aggregateId,
            EventVersion expectedVersion, ICollection<EventBase> events, CancellationToken cancellationToken);

        Task<IEnumerable<EventBase>> GetEventsForAggregate(Guid aggregateId);


        Task<bool> Init(bool createTableIfNotExists);

        Task<IEnumerable<EventBase>> ReadEventsAsync(IEnumerable<CommitLogEntity> logs);

    }
}
