
using SuperNova.Shared.Configs;
using SuperNova.Shared.Repositories;
using SuperNova.Shared.Supports;
using SuperNova.Storage.Repositories;
using SuperNova.Storage.Supports;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using SuperNova.Shared.DomainObjects;

namespace SuperNova.Storage
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private ConfigStore _configStore;
        private ILoggerFactory _logFactory;

        private Dictionary<Type, Type> _typeMap = new Dictionary<Type, Type>();

        public RepositoryFactory(ConfigStore configStore, ILoggerFactory logFactory)
        {
            Ensure.ArgumentNotNull(configStore, nameof(configStore));
            Ensure.ArgumentNotNull(logFactory, nameof(logFactory));

            this._configStore = configStore;
            this._logFactory = logFactory;

            this.DiscoverRepositories();
        }

        // Not thread - safe (Be careful)
        protected virtual void DiscoverRepositories()
        {
            foreach (var repoType in typeof(RepositoryFactory).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
            {
                if (repoType.IsSubclassOf(typeof(TableStoreBase)) ||
                    repoType.IsSubclassOf(typeof(DocumentStoreBase)))
                {
                    foreach (var interfaceType in repoType.GetInterfaces().Where(t => !t.Equals(typeof(IRepository))))
                    {
                        _typeMap[interfaceType] = repoType;
                    }
                }
            }
        }

        public async Task<TTableStore> CreateTableRepositoryAsync<TTableStore>(Tenant tenant) where TTableStore : IRepository
        {
            if (!_typeMap.ContainsKey(typeof(TTableStore)))
            {
                new ArgumentOutOfRangeException($"{typeof(TTableStore).FullName} is not registered.");
            }

            return (TTableStore)(await RepositoryExtensions
                .CreateTableRepository(_typeMap[typeof(TTableStore)], tenant, this._configStore, this._logFactory)
                .ConfigureAwait(false));
        }

        public async Task<TDocumentStore> CreateDocumentRepositoryAsync<TDocumentStore>(Tenant tenant) where TDocumentStore : IRepository
        {
            if (!_typeMap.ContainsKey(typeof(TDocumentStore)))
            {
                new ArgumentOutOfRangeException($"{typeof(TDocumentStore).FullName} is not registered.");
            }

            return (TDocumentStore)(await RepositoryExtensions
                .CreateDocumentRepository(_typeMap[typeof(TDocumentStore)], tenant, this._configStore, this._logFactory)
                .ConfigureAwait(false));
        }
    }
}
