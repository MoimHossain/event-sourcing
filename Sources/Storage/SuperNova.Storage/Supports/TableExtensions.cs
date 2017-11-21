

using SuperNova.Shared.Supports;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SuperNova.Storage.Supports.StorageConstants;

namespace SuperNova.Storage.Supports
{
    public static class TableExtensions
    {
        public static async Task RemoveAsync(
            this CloudTable table,
            KeysPair keys)
        {
            var entity = new DynamicTableEntity(keys.PartitionKey, keys.RowKey);
            entity.ETag = "*";
            await table.ExecuteAsync(TableOperation.Delete(entity))
               .ConfigureAwait(false);
        }

        public static async Task<DynamicTableEntity> Insert(
            this CloudTable table,
            DynamicTableEntity dte)            
        {            
            Ensure.ArgumentNotNull(dte, nameof(dte));
            Ensure.ArgumentNotNull(table, nameof(table));

            dte.Timestamp = DateTime.UtcNow;

            var result = await table.ExecuteAsync(TableOperation.Insert(dte)).ConfigureAwait(false);

            return (result.Result as DynamicTableEntity);
        }

        public static async Task AddAsync<TPayload>(
            this CloudTable table, TPayload payload,
            KeysPair keys)
            where TPayload : class, new()
        {
            Ensure.ArgumentNotNull(keys, nameof(keys));
            Ensure.ArgumentNotNull(payload, nameof(payload));
            Ensure.ArgumentNotNull(table, nameof(table));

            var entity = new DynamicTableEntity(keys.PartitionKey, keys.RowKey);            
            entity.Timestamp = DateTime.UtcNow;            
            entity.Properties = EntityPropertyConverter.Flatten(payload, new OperationContext());
            var batch = new TableBatchOperation();
            batch.Insert(entity);

            await table.ExecuteBatchAsync(batch)
                .ConfigureAwait(false);
        }
        

        public static async Task UpdateAsync<TPayload>(
            this CloudTable table, TPayload payload,
            KeysPair keys)
            where TPayload : class, new()
        {
            Ensure.ArgumentNotNull(keys, nameof(keys));
            Ensure.ArgumentNotNull(payload, nameof(payload));
            Ensure.ArgumentNotNull(table, nameof(table));
            
            var entity = new DynamicTableEntity(keys.PartitionKey, keys.RowKey);
            entity.Timestamp = DateTime.UtcNow;
            entity.Properties = EntityPropertyConverter.Flatten(payload, new OperationContext());
            entity.ETag = "*";

            var batch = new TableBatchOperation();
            batch.Replace(entity);

            await table.ExecuteBatchAsync(batch)
                .ConfigureAwait(false);

            await table.ExecuteBatchAsync(batch)
               .ConfigureAwait(false);
        }

        public static async Task<DynamicTableEntity> GetAsync(
            this CloudTable table, string partitionKey, string rowKey)            
        {
            Ensure.ArgumentNotNullOrWhiteSpace(partitionKey, nameof(partitionKey));
            Ensure.ArgumentNotNullOrWhiteSpace(rowKey, nameof(rowKey));
            Ensure.ArgumentNotNull(table, nameof(table));

            return await table.RetrieveAsync(partitionKey, rowKey).ConfigureAwait(false);
        }

        public static async Task<TPayload> GetAsync<TPayload>(
            this CloudTable table, string partitionKey, string rowKey)
            where TPayload : class, new()
        {
            return await (await GetAsync(table, partitionKey, rowKey))
                .Unwrap<TPayload>().ConfigureAwait(false); 
        }

        public static async Task<TPayload> Unwrap<TPayload>
                    (this DynamicTableEntity dte, string columnName = Columns.Json)
        {
            if (dte == null)
            {
                return default(TPayload);
            }

            return await Task.Factory.StartNew<TPayload>(() =>
            {
                var payload = 
                    EntityPropertyConverter.ConvertBack<TPayload>
                        (dte.Properties, new OperationContext());                
                return payload;
            }).ConfigureAwait(false);
        }

        public static async Task<CloudTable> CreateTableClientAsync(
            StorageCredentials credentials, string tableName, bool createTableIfNotExists, ILogger logger)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(tableName, nameof(tableName));
            Ensure.ArgumentNotNull(credentials, nameof(credentials));
            Ensure.ArgumentNotNull(logger, nameof(logger));

            var table = default(CloudTable);
            await SafetyExtensions.ExecuteAsync(logger, async () =>
            {   
                var storageAccount = new CloudStorageAccount(credentials, true);
                var tableClient = storageAccount.CreateCloudTableClient();
                table = tableClient.GetTableReference(tableName);
                if (createTableIfNotExists)
                {
                    await table.CreateIfNotExistsAsync()
                        .ConfigureAwait(false);
                }
            });
            return table;
        }

        public static async Task<DynamicTableEntity> RetrieveAsync(
            this CloudTable table, string partitionKey, string rowKey, bool suppressError = false)
        {
            Ensure.ArgumentNotNull(table, nameof(table));
            Ensure.ArgumentNotNullOrWhiteSpace(partitionKey, nameof(partitionKey));
            Ensure.ArgumentNotNullOrWhiteSpace(rowKey, nameof(rowKey));

            var tableResult = default(TableResult);
            try
            {
                tableResult = await table.ExecuteAsync(
                                    TableOperation.Retrieve(partitionKey, rowKey))
                                    .ConfigureAwait(false);
            }
            catch(StorageException se)
            {
                if(!suppressError)
                {
                    throw se;
                }
            }
            if (tableResult != null && tableResult.Result != null
                && tableResult.Result is DynamicTableEntity)
            {
                return tableResult.Result as DynamicTableEntity;
            }
            return default(DynamicTableEntity);
        }

        public static async Task<IEnumerable<DynamicTableEntity>> GetAll(
            this CloudTable table, string partitionKey, string rowKey = null, 
            TableContinuationToken token = null)
        {
            Ensure.ArgumentNotNull(table, nameof(table));            

            TableQuery query;

            if (string.IsNullOrWhiteSpace(rowKey))
            {
                query = new TableQuery()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            }
            else
            {
                query = new TableQuery().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey)));
            }            
            var resultSegment = await table.ExecuteQuerySegmentedAsync(query, token)
                .ConfigureAwait(false);
            // Ignoring the token for now. 
            return resultSegment.Results;
        }
    }

    
}

