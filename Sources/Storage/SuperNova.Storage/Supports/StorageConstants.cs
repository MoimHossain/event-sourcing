using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNova.Storage.Supports
{
    public static class StorageConstants
    {
        public const string TableAccountName = "storage-table-account-name";
        public const string TableAccountKey = "storage-table-account-key";

        public const string DocumentEndpoint = "storage-document-account-endpoint";
        public const string DocumentKey = "storage-document-account-key";

        public const string PARTITION_KEY = "PartitionKey";
        public const string ROWKEY = "RowKey";

        public static class Exceptions
        {
            public const string FailedToCreateEventStream = "An error occured while creating event stream handle.";
        }

        public static class EventStore
        {
            public static class CommitLogs
            {
                public const string CommitPartitionValue = "$commit";
                public const string SequencePartitionValue = "$metadata";
                public const string SequenceRowValue = "$sequence-number";

                public const string SequenceColumnName = "SequenceNumber";
            }

            public static class Commits
            {
                public const string CommitId = "CommitId";
                public const string FromVersion = "FromVersion";
                public const string ToVersion = "ToVersion";
                public const string AggregateId = "AggregateId";
                public const string StreamName = "StreamName";
            }

            public const string EventType = "EventType";
            public const string EventData = "EventData";

            public const string LatestCommitID = "LatestCommitID";
            public const string VersionRowValue = "$version";
            public const string VersionColumn = "version";
            public static string EtagZero { get => "0"; }
        }

        public static class Tables
        {            
            public const string Tenants = "tenants";
            public const string LeaseTable = "leasecollection";
        }

        public static class Columns
        {
            public const string Json = "JSON";
            public const string Id = "Id";
            public const string TenantId = "TenantId";
            public const string OffsetCommitId = "OffsetCommitId";
        }


    }
}
