using BrotliSharpLib;
using SuperNova.Shared.EventStore;
using SuperNova.Shared.EventStore.Transactions;
using SuperNova.Shared.Messaging;

using SuperNova.Shared.Supports;
using SuperNova.Storage.EventStore;
using SuperNova.Storage.EventStore.Transactions;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SuperNova.Shared.DomainObjects;

namespace SuperNova.Storage.Supports
{
    public static  class EventExtensions
    {   

        public static CommitLogEntity ToCommitLog(this DynamicTableEntity dte)
        {
            Ensure.ArgumentNotNull(dte, nameof(dte));

            return new CommitLogEntity
            {
                PartitionKey = dte.PartitionKey,
                RowKey = dte.RowKey,
                CommitId = dte.Properties[StorageConstants.EventStore.Commits.CommitId].Int64Value.Value,
                FromVersion = dte.Properties[StorageConstants.EventStore.Commits.FromVersion].Int64Value.Value,
                ToVersion = dte.Properties[StorageConstants.EventStore.Commits.ToVersion].Int64Value.Value,
                AggregateId = dte.Properties[StorageConstants.EventStore.Commits.AggregateId].GuidValue.Value,
                StreamName = dte.Properties[StorageConstants.EventStore.Commits.StreamName].StringValue
            };
        }

        public static async Task<ITransaction> BeginTransactionAsync(
            this IEventStore eventStore,
            Tenant tenant, string streamName, Guid aggregateId)
        {
            var scopedTransaction =
                new ScopedTransaction(
                    (await eventStore.GetStreamAsync(tenant, streamName)),
                    aggregateId);

            await scopedTransaction.CreateAsync();
            return scopedTransaction;
        }

        public static async Task ExecuteInTransactionAsync(
            this IEventStore store,
            Tenant tenant, string streamName, Guid aggregateId,
            Func<ITransaction, Task> work)
        {
            var tx = await store.BeginTransactionAsync(tenant, streamName, aggregateId);

            await work(tx);

            await tx.CommitAsync();
        }

        public static string FormatLong(this long number)
        {
            return number.ToString("000000000000000000000");
        }

        public static DynamicTableEntity ToEntity(
            this EventVersion version, string aggregateId, long commitId)
        {
            Ensure.ArgumentNotNull(version, nameof(version));
            Ensure.ArgumentNotNullOrWhiteSpace(aggregateId, nameof(aggregateId));            

            var dte = new DynamicTableEntity(aggregateId, StorageConstants.EventStore.VersionRowValue);

            dte.Properties = new Dictionary<string, EntityProperty>();
            dte.Properties.Add(StorageConstants.EventStore.VersionColumn,
                EntityProperty.GeneratePropertyForLong(version.Version));
            dte.Properties.Add(StorageConstants.EventStore.LatestCommitID,
                EntityProperty.GeneratePropertyForLong(commitId));
            // Most important thing here 
            dte.ETag = version.ETag;

            return dte;
        }

        public static EventBase FromEntity(this DynamicTableEntity dte)
        {
            Ensure.ArgumentNotNull(dte, nameof(dte));

            var jsonString = Encoding.Default
                .GetString(dte.Properties
                    [StorageConstants.EventStore.EventData].BinaryValue);
            var type =
                Type.GetType(
                    dte.Properties[StorageConstants.EventStore.EventType].StringValue, true);
            var @event = (EventBase)JsonConvert.DeserializeObject(jsonString, type);

            @event.AggregateId = Guid.Parse(dte.PartitionKey);
            return @event;
        }

        public static DynamicTableEntity ToEntity(
            this EventBase @event, string partitionkey, long eventVersion)
        {
            Ensure.ArgumentNotNull(@event, nameof(@event));
            var dte = new DynamicTableEntity(partitionkey, eventVersion.FormatLong());

            dte.Properties = new Dictionary<string, EntityProperty>();
            dte.Properties.Add(StorageConstants.EventStore.EventType, 
                EntityProperty.GeneratePropertyForString(@event.GetType().AssemblyQualifiedName));

            var bytes = Encoding.Default.GetBytes(JsonConvert.SerializeObject(@event));
            // var compressed = Brotli.CompressBuffer(bytes, 0, bytes.Length);

            dte.Properties.Add(StorageConstants.EventStore.EventData,
                EntityProperty.GeneratePropertyForByteArray(bytes));

            return dte;
        }
    }
}
