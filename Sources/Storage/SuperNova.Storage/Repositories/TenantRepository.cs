
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using SuperNova.Storage.Supports;
using SuperNova.Shared.Repositories;
using System.Threading.Tasks;
using SuperNova.Shared.Supports;
using Microsoft.WindowsAzure.Storage.Auth;
using SuperNova.Shared.DomainObjects;

namespace SuperNova.Storage.Repositories
{
    public class TenantRepository 
        : TableStoreBase, ITenantRepository
    {
        public TenantRepository(
            StorageCredentials credentials, 
            ILoggerFactory factory) 
            : base(StorageConstants.Tables.Tenants, credentials, factory, true)
        {

        }

        private KeysPair GetKeys(Tenant tenant)
        {
            if(tenant != null)
            {
                return new KeysPair(
                    tenant.TenantId.ToSafeStorageKey(),
                    tenant.TenantId.ToSafeStorageKey());
            }
            return default(KeysPair);
        }

        public async Task<Tenant> GetAsync(Guid tenantId)
        {
            var tenant = await Table.GetAsync<Tenant>(
                tenantId.ToSafeStorageKey(), tenantId.ToSafeStorageKey())
                .ConfigureAwait(false);

            return tenant;
        }

        public async Task SaveAsync(Tenant tenant)
        {
            Ensure.ArgumentNotNull(tenant, nameof(tenant));

            await Table.AddAsync<Tenant>(tenant, GetKeys(tenant)).ConfigureAwait(false);
        }
    }
}
