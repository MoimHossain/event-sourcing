using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNova.StorageTest.FakeStorage
{
    public class FakeCloudTable : CloudTable
    {
        private Func<TableOperation, Task<TableResult>> _proxyFunc;
        public FakeCloudTable() : base(new Uri("https://SuperNova.nl"))
        {

        }

        public FakeCloudTable(Func<TableOperation, Task<TableResult>> proxyFunc) : this()
        {
            _proxyFunc = proxyFunc;
        }

        public override async Task<bool> CreateIfNotExistsAsync()
        {
            return await Task.FromResult<Boolean>(true);
        }

        public override async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            if(_proxyFunc != null )
            {
                return await _proxyFunc(operation);
            }

            return await Task.FromResult<TableResult>(
                new TableResult {
                    Result = new object()
                });
        }

        public override async Task<TableQuerySegment> ExecuteQuerySegmentedAsync(TableQuery query, TableContinuationToken token)
        {
            return await Task.FromResult<TableQuerySegment>(null);
        }

        public override Task<IList<TableResult>> ExecuteBatchAsync(TableBatchOperation batch)
        {
            foreach( var op in batch)
            {
                this.ExecuteAsync(op).Wait();
            }

            return Task.FromResult<IList<TableResult>>(new List<TableResult>());
        }
    }
}
