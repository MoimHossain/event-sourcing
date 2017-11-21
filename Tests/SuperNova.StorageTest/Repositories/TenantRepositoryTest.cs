
using SuperNova.Shared.Tests;
using SuperNova.Shared.Tests.TestUtils;
using SuperNova.Storage;
using SuperNova.Storage.Repositories;
using SuperNova.Storage.Supports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNova.StorageTest.Repositories
{
    [TestClass]
    public class TenantRepositoryTest
    {
        private TenantRepository repository = default(TenantRepository);        
        private List<ITableEntity> testTenants = new List<ITableEntity>();
        private Dictionary<string, ITableEntity> store = new Dictionary<string, ITableEntity>();

        [TestInitialize]
        public async Task Init()
        {
            var configStore = new TestConfigStore(new FakeLogFactory());

            var credentials = new StorageCredentials(
                (await configStore.GetAsync(StorageConstants.TableAccountName)),
                (await configStore.GetAsync(StorageConstants.TableAccountKey)));
            repository = new FakeTenantRepository
                (credentials,
                new FakeLogFactory(),
                (tOp) =>
                {                    
                    
                    if (tOp.OperationType == TableOperationType.Retrieve)
                    {
                        var pkey = TestUtils.GetInstanceProperty(tOp.GetType(), tOp, "PartitionKey");
                        var rkey = TestUtils.GetInstanceProperty(tOp.GetType(), tOp, "RowKey");
                        var key = string.Format("{0}-{1}", pkey, rkey);
                        if (store.ContainsKey(key))
                        {
                            return Task.FromResult<TableResult>(new TableResult()
                            {
                                Result = store[key]
                            });
                        }
                    }
                    else if(tOp.OperationType == TableOperationType.InsertOrReplace)
                    {
                        var key = string.Format("{0}-{1}", tOp.Entity.PartitionKey, tOp.Entity.RowKey);
                        store[key] = tOp.Entity;
                    }
                    return Task.FromResult(new TableResult());
                });
        }

    }
}
