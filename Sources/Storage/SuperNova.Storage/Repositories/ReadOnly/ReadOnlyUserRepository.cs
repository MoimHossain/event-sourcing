
using System.Threading.Tasks;
using SuperNova.Shared.Configs;
using SuperNova.Shared.DomainObjects;
using SuperNova.Shared.Repositories.Materialize;
using SuperNova.Shared.Repositories.ReadOnly;
using SuperNova.Shared.Supports;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using SuperNova.Shared.Dtos;

namespace SuperNova.Storage.Repositories.ReadOnly
{
    public class ReadOnlyUserRepository 
        : DocumentStoreBase, 
        IReadOnlyUserRepository, 
        IUserMaterialzeRepository
    {
        private Tenant _tenant;
        public ReadOnlyUserRepository
            (Tenant tenant, ConfigStore configStore, ILoggerFactory logFactory) 
            : base(configStore, logFactory)
        {
            Ensure.ArgumentNotNull(tenant, nameof(tenant));

            this._tenant = tenant;
        }

        protected override string Database => $"qdb{_tenant.TenantId.ToLowercaseAlphaNum()}";

        protected override string Collection => "requirements";

        public async Task AddUserAsync(UserDto currentState, long version)
        {
            await base.CreateAsync<UserDto>(currentState).ConfigureAwait(false);
        }

        public async Task UpdateUserAsync(UserDto currentState, long version)
        {
            await base.ReplaceAsync<UserDto>(currentState.UserId, currentState);
        }

        public virtual async Task<IEnumerable<UserDto>> QueryAsync
            (Expression<Func<UserDto, bool>> predicate)
        {
            return await base.QueryAsync<UserDto>(predicate);
        }
    }
}
