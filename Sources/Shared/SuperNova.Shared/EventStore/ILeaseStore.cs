using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNova.Shared.EventStore
{
    public interface ILeaseStore
    {
        Task<bool> InitAsync();
        Task SetOffsetCommitIdAsync(long offsetCommitId);
        Task<long?> TryGetOffsetCommitIdAsync();
    }
}
