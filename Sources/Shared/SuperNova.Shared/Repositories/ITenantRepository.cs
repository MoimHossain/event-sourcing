using SuperNova.Shared.DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNova.Shared.Repositories
{
    public interface ITenantRepository
    {
        Task SaveAsync(Tenant tenant);

        Task<Tenant> GetAsync(Guid tenantId);
    }
}
