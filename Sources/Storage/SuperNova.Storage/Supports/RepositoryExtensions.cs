

using SuperNova.Shared.Configs;
using SuperNova.Shared.EventStore;
using SuperNova.Shared.Repositories;
using SuperNova.Shared.Supports;
using SuperNova.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SuperNova.Shared.DomainObjects;

namespace SuperNova.Storage.Supports
{
    public static class RepositoryExtensions
    {
        public static async Task<IRepository>
            CreateTableRepository(
            Type repoType,
            Tenant tenant,
            ConfigStore configStore,
            ILoggerFactory logFactory)            
        {
            Ensure.ArgumentNotNull(tenant, nameof(tenant));
            Ensure.ArgumentNotNull(logFactory, nameof(logFactory));

            var credentials = new StorageCredentials(
                (await configStore.GetAsync(StorageConstants.TableAccountName).ConfigureAwait(false)),
                (await configStore.GetAsync(StorageConstants.TableAccountKey).ConfigureAwait(false)));

            var repository = (IRepository)Activator.CreateInstance(repoType, credentials, logFactory);

            if (!(await repository.Init().ConfigureAwait(false)))
            {
                // throw exception here? 
            }
            return repository;
        }

        public static async Task<IRepository>
            CreateDocumentRepository(
            Type repoType,
            Tenant tenant,
            ConfigStore configStore,
            ILoggerFactory logFactory)            
        {
            Ensure.ArgumentNotNull(logFactory, nameof(logFactory));
            
            var repository = (IRepository)Activator.CreateInstance(repoType, tenant, configStore, logFactory);

            if (!(await repository.Init().ConfigureAwait(false)))
            {
                // throw exception here? 
            }
            return repository;
        }
    }
}
