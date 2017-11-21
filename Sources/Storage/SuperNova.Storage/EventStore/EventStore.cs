
using SuperNova.Shared.Configs;
using SuperNova.Shared.EventStore;
using SuperNova.Shared.Supports;
using SuperNova.Storage.Supports;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Threading.Tasks;
using static SuperNova.Storage.Supports.StorageConstants;
using SuperNova.Shared.EventStore.Transactions;
using SuperNova.Storage.EventStore.Transactions;
using SuperNova.Shared.DomainObjects;

namespace SuperNova.Storage.EventStore
{
    public class EventStore : IEventStore
    {
        private ILogger<EventStore> _logger;
        private ConfigStore _configStore;
        private ILoggerFactory _factory;

        public EventStore(ConfigStore configStore, ILoggerFactory factory)
        {
            Ensure.ArgumentNotNull(configStore, nameof(configStore));
            Ensure.ArgumentNotNull(factory, nameof(factory));

            this._configStore = configStore;
            this._factory = factory;
            this._logger = factory.CreateLogger<EventStore>();
        }

        public virtual async Task<IEventStream> GetStreamAsync(Tenant tenant, string streamName)
        {
            var credentials = new StorageCredentials(
                (await this._configStore.GetAsync(StorageConstants.TableAccountName).ConfigureAwait(false)),
                (await this._configStore.GetAsync(StorageConstants.TableAccountKey).ConfigureAwait(false)));

            var commitLogs = new CommitTrailStore(credentials, tenant, _factory);
            var stream = new EventStream(credentials, tenant, streamName, commitLogs, _factory);

            if (!((await commitLogs.InitAsync().ConfigureAwait(false)) && (await stream.Init().ConfigureAwait(false))))
            {
                throw new InvalidProgramException(Exceptions.FailedToCreateEventStream);
            }
            return stream;
        }        
    }
}
