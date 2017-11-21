using SuperNova.Storage.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using SuperNova.StorageTest.FakeStorage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace SuperNova.StorageTest.Repositories
{
    public class FakeTenantRepository : TenantRepository
    {
        private Func<TableOperation, Task<TableResult>> _proxyFunc;

        public FakeTenantRepository(
            StorageCredentials credentails, ILoggerFactory factory,
            Func<TableOperation, Task<TableResult>> proxyFunc)
            : base(credentails, factory)
        {
            this._proxyFunc = proxyFunc;
        }

        public FakeTenantRepository(
            StorageCredentials credentails, ILoggerFactory factory) 
            : base(credentails, factory)
        {

        }

        public override async Task<bool> Init()
        {
            return await Task.FromResult(true);
        }



        protected override CloudTable Table => _proxyFunc != null 
            ? new FakeCloudTable(_proxyFunc) : new FakeCloudTable();
    }
}
