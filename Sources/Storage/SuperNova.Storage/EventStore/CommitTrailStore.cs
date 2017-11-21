
using SuperNova.Shared.EventStore;
using SuperNova.Shared.Supports;
using SuperNova.Storage.Supports;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;
using SuperNova.Shared.Messaging;
using SuperNova.Shared.DomainObjects;

namespace SuperNova.Storage.EventStore
{
    public class CommitTrailStore : ICommitTrailStore
    {
        private CloudTable _table;
        private StorageCredentials _credentials;
        private ILogger _logger;        
        private Tenant _tenant;

        public CommitTrailStore(
            StorageCredentials credentials,
            Tenant tenant,
            ILoggerFactory loggerFactory)
        {            
            Ensure.ArgumentNotNull(tenant, nameof(tenant));
            Ensure.ArgumentNotNull(credentials, nameof(credentials));
            Ensure.ArgumentNotNull(loggerFactory, nameof(loggerFactory));

            this._tenant = tenant;
            this._credentials = credentials;
            this._logger = loggerFactory.CreateLogger<CommitTrailStore>();
        }

        public virtual async Task<bool> InitAsync()
        {
            _table = await TableExtensions
                .CreateTableClientAsync(
                    _credentials, TableName, true, _logger)
                    .ConfigureAwait(false);

            return _table != null;
        }

        protected virtual string TableName { get => $"commitlogs{this._tenant.TenantId.ToSafeStorageKey()}"; }
        
        public virtual async Task<long> GetCurrentCommitIdAsync()
        {
            long commitSequence = 1;

            var commitSequenceEntity = await this._table.RetrieveAsync(
                            StorageConstants.EventStore.CommitLogs.SequencePartitionValue,
                            StorageConstants.EventStore.CommitLogs.SequenceRowValue, true)
                            .ConfigureAwait(false);
            if (commitSequenceEntity != null)
            {
                commitSequence = commitSequenceEntity
                    .Properties[StorageConstants.EventStore.CommitLogs.SequenceColumnName]
                    .Int64Value.Value;
            }
            return commitSequence;
        }

        public virtual async Task<long> GetNextCommitIdAsync()
        {
            var commitId = default(long?);

            do
            {
                commitId = await TryGetNextCommitIdAsync().ConfigureAwait(false);
            }
            while (!commitId.HasValue);

            return commitId.Value;
        }

        private async Task<long?> TryGetNextCommitIdAsync()
        {
            long? nextCommitSequence = 1;

            var commitSequenceEntity = await this._table.RetrieveAsync(
                            StorageConstants.EventStore.CommitLogs.SequencePartitionValue,
                            StorageConstants.EventStore.CommitLogs.SequenceRowValue, true)
                            .ConfigureAwait(false);

            if (commitSequenceEntity == null)
            {
                commitSequenceEntity = new DynamicTableEntity
                {
                    PartitionKey = StorageConstants.EventStore.CommitLogs.SequencePartitionValue,
                    RowKey = StorageConstants.EventStore.CommitLogs.SequenceRowValue,
                    Properties = new Dictionary<string, EntityProperty>
                    {
                        { StorageConstants.EventStore.CommitLogs.SequenceColumnName,
                            EntityProperty.GeneratePropertyForLong(nextCommitSequence)  }
                    }
                };
            }
            else
            {
                nextCommitSequence = commitSequenceEntity.Properties[StorageConstants.EventStore.CommitLogs.SequenceColumnName].Int64Value + 1;
                commitSequenceEntity.Properties[StorageConstants.EventStore.CommitLogs.SequenceColumnName].Int64Value = nextCommitSequence;
            }

            try
            {
                await this._table.ExecuteAsync(TableOperation.InsertOrReplace(commitSequenceEntity))
                    .ConfigureAwait(false);
                return nextCommitSequence;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode != (int)HttpStatusCode.Conflict)
                {
                    throw;
                }
                // We are eatinig the conflict exceptions here
            }

            return null;
        }

        public virtual async Task<IEnumerable<CommitLogEntity>> GetCommitsAsync(
            long offsetCommitId, long recentCommitId)
        {
            var rowKeyClause = string.Empty;
            var commitLogs = new List<CommitLogEntity>();

            for (var start = offsetCommitId; start <= recentCommitId; ++start)
            {
                var nq = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, start.FormatLong());
                rowKeyClause = !string.IsNullOrWhiteSpace(rowKeyClause) 
                    ? TableQuery.CombineFilters(rowKeyClause.ToString(), TableOperators.Or, nq) : nq;
            }

            var whereClause =
                TableQuery.CombineFilters(
                    TableQuery
                        .GenerateFilterCondition(
                            "PartitionKey", QueryComparisons.Equal,
                            StorageConstants.EventStore.CommitLogs.CommitPartitionValue),
                    TableOperators.And,
                    rowKeyClause
                );

            var resultSegment =
                await this._table.ExecuteQuerySegmentedAsync(new TableQuery().Where(whereClause), null)
                .ConfigureAwait(false);

            return resultSegment.Results.Select(dte => dte.ToCommitLog());
        }

        public async Task AnnounceCommitAsync(
            long commitId, Guid aggregateId, string streamName, EventVersion version, 
            ICollection<EventBase> events, CancellationToken cancellationToken)
        {
            var dte = new DynamicTableEntity(
                StorageConstants.EventStore.CommitLogs.CommitPartitionValue,
                commitId.FormatLong());

            dte.Properties = new Dictionary<string, EntityProperty>
            {
                { StorageConstants.EventStore.Commits.CommitId, EntityProperty.GeneratePropertyForLong(commitId) },
                { StorageConstants.EventStore.Commits.FromVersion, EntityProperty.GeneratePropertyForLong(version.Version + 1) },
                { StorageConstants.EventStore.Commits.ToVersion, EntityProperty.GeneratePropertyForLong(version.Version + events.Count) },
                { StorageConstants.EventStore.Commits.AggregateId, EntityProperty.GeneratePropertyForGuid(aggregateId) },
                { StorageConstants.EventStore.Commits.StreamName, EntityProperty.GeneratePropertyForString(streamName) }
            };

            await this._table.ExecuteAsync(TableOperation.Insert(dte)).ConfigureAwait(false);
        }
    }
}
