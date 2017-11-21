

using SuperNova.Shared.EventStore;
using SuperNova.Shared.Exceptions;

using SuperNova.Shared.Supports;
using SuperNova.Storage.Supports;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;
using SuperNova.Shared.EventStore.Transactions;
using SuperNova.Shared.Messaging;
using SuperNova.Shared.DomainObjects;

namespace SuperNova.Storage.EventStore
{
    public class EventStream : IEventStream
    {
        private CloudTable _table;
        private StorageCredentials _credentials;
        private ILogger _logger;
        private string _streamName;
        private Tenant _tenant;
        private ICommitTrailStore _commitLogs;

        public EventStream(
            StorageCredentials credentials, 
            Tenant tenant,
            string streamName,
            ICommitTrailStore commitLogs,
            ILoggerFactory loggerFactory)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(streamName, nameof(streamName));
            Ensure.ArgumentNotNull(tenant, nameof(tenant));
            Ensure.ArgumentNotNull(credentials, nameof(credentials));
            Ensure.ArgumentNotNull(loggerFactory, nameof(loggerFactory));
            Ensure.ArgumentNotNull(commitLogs, nameof(commitLogs));

            this._streamName = streamName;
            this._tenant = tenant;
            this._credentials = credentials;
            this._commitLogs = commitLogs;
            this._logger = loggerFactory.CreateLogger<EventStream>();
        }

        public virtual async Task<bool> Init(bool createTableIfNotExists = true)
        {   
            _table = await TableExtensions
                .CreateTableClientAsync(
                    _credentials, TableName, createTableIfNotExists, _logger)
                    .ConfigureAwait(false);

            return _table != null;
        }

        protected virtual string TableName { get => $"{this._streamName}{this._tenant.TenantId.ToSafeStorageKey()}"; }
        

        public virtual async Task EmitEventsAsync(
            Guid aggregateId,
            EventVersion expectedVersion, ICollection<EventBase> events, CancellationToken cancellationToken)
        {
            Ensure.ArgumentNotNull(expectedVersion, nameof(expectedVersion));            
            Ensure.ArgumentNotNull(events, nameof(events));
            if(events.Count <= 0 )
            {
                throw new ArgumentOutOfRangeException("At lease one event should be present.");
            }
            
            var currentVersion = await GetCurrentVersionAsync(aggregateId);

            if(currentVersion.Version != expectedVersion.Version)
            {
                throw new OptimisticConcurrencyException(string.Format($"Stream has been moved beyond expected version ({expectedVersion.Version})."));
            }

            var commitId = await this._commitLogs.GetNextCommitIdAsync().ConfigureAwait(false);

            // First create a dirty commit entity to announce the fact that we have the intention to add
            // some events. Events won't actually be dispatched until they're successfully persisted
            // in the stream.
            // The dirty commit entities are used by the chaser process to dispatch the commits.
            await this._commitLogs.AnnounceCommitAsync(
                commitId, aggregateId, _streamName, expectedVersion, events, cancellationToken)
                .ConfigureAwait(false);

            // Now add the events to actual stream. If this fails the commit will expire
            // and be cleaned up by the chaser.
            await this.AppendEventsAsync
                (aggregateId, commitId, currentVersion, events, cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task<IList<DynamicTableEntity>> AppendEventsAsync(
            Guid aggregateId, long commitId, 
            EventVersion currentVersion, 
            ICollection<EventBase> events, CancellationToken cancellationToken)
        {
            var batchOp = new TableBatchOperation();            

            foreach(var @event in events )
            {
                batchOp.Add(TableOperation.InsertOrReplace
                    (@event.ToEntity(aggregateId.ToLowercaseAlphaNum(), (currentVersion.Version += 1))));
            }

            batchOp.Add(TableOperation.InsertOrReplace
                (currentVersion.ToEntity(aggregateId.ToLowercaseAlphaNum(), commitId)));

            var data = (await this._table.ExecuteBatchAsync(batchOp).ConfigureAwait(false))
                .Select(r => r.Result as DynamicTableEntity)
                .ToList();

            return data;
        }

        public virtual async Task<IEnumerable<EventBase>> ReadEventsAsync(IEnumerable<CommitLogEntity> logs)
        {
            Ensure.ArgumentNotNull(logs, nameof(logs));

            var whereClause = string.Empty;
            foreach (var log in logs)
            {
                var clause = TableQuery.CombineFilters(
                    TableQuery
                    .GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, log.AggregateId.ToLowercaseAlphaNum()),
                    TableOperators.And,
                    TableQuery.CombineFilters(
                    TableQuery
                    .GenerateFilterCondition("RowKey",
                        QueryComparisons.GreaterThanOrEqual, log.FromVersion.FormatLong()),
                    TableOperators.And,
                    TableQuery
                    .GenerateFilterCondition("RowKey",
                        QueryComparisons.LessThanOrEqual, log.ToVersion.FormatLong())));

                whereClause = string.IsNullOrWhiteSpace(whereClause) ? clause :
                    TableQuery.CombineFilters(whereClause, TableOperators.Or, clause);
            }

            var resultSegment =
                await this._table.ExecuteQuerySegmentedAsync(new TableQuery().Where(whereClause), null)
                .ConfigureAwait(false);

            return resultSegment.Results.Select(dte => dte.FromEntity());
        }

        public virtual async Task<EventVersion> GetCurrentVersionAsync(Guid aggregateId)
        {
            var partitionKey = aggregateId.ToLowercaseAlphaNum();
            var rowKey = StorageConstants.EventStore.VersionRowValue;

            var dte 
                = await this._table.RetrieveAsync(partitionKey, rowKey, true)
                .ConfigureAwait(false);

            if (dte == null)
            {
                dte = new DynamicTableEntity(partitionKey, rowKey);
                dte.Properties = new Dictionary<string, EntityProperty> {
                        {  StorageConstants.EventStore.VersionColumn, EntityProperty.GeneratePropertyForLong(0) }
                    };
                dte = await this._table.Insert(dte).ConfigureAwait(false);
            }
            return new EventVersion(dte.Properties[StorageConstants.EventStore.VersionColumn].Int64Value.Value, dte.ETag);
        }

        // TODO: This method should be optimized later on to avoid partition scan
        public async virtual Task<IEnumerable<EventBase>> GetEventsForAggregate(Guid aggregateId)
        {
            var whereClause = TableQuery.CombineFilters(
                TableQuery
                    .GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, aggregateId.ToLowercaseAlphaNum()),
                TableOperators.And,
                TableQuery
                .GenerateFilterCondition("RowKey", QueryComparisons.NotEqual,
                StorageConstants.EventStore.VersionRowValue));

            var resultSegment =
                await this._table.ExecuteQuerySegmentedAsync(new TableQuery().Where(whereClause), null)
                .ConfigureAwait(false);

            return resultSegment.Results.Select(dte => dte.FromEntity());
        }
    }
}
