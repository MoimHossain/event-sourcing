

using SuperNova.Shared.Messaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNova.Shared.EventStore
{
    /// <summary>
    /// API interface to the commit trail storage.
    /// </summary>
    public interface ICommitTrailStore
    {
        Task<long> GetNextCommitIdAsync();
        Task AnnounceCommitAsync(
            long commitId, Guid aggregateId, string streamName,
            EventVersion expectedVersion, ICollection<EventBase> events, 
            CancellationToken cancellationToken);
        Task<long> GetCurrentCommitIdAsync();
        Task<IEnumerable<CommitLogEntity>> GetCommitsAsync(
            long offsetCommitId, long recentCommitId);
    }
}
