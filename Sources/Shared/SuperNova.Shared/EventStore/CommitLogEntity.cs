

using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNova.Shared.EventStore
{
    /// <summary>
    /// Represents a single commit trail record
    /// </summary>
    public class CommitLogEntity
    {
        public string PartitionKey { get;  set; }
        public string RowKey { get;  set; }
        public long CommitId { get;  set; }
        public long FromVersion { get;  set; }
        public long ToVersion { get;  set; }
        public Guid AggregateId { get;  set; }
        public string StreamName { get;  set; }
    }
}
