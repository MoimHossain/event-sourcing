using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using SuperNova.Shared.Configs;
using SuperNova.Shared.DomainObjects;
using SuperNova.Shared.EventStore;
using SuperNova.Shared.Messaging;
using SuperNova.Shared.Supports;
using SuperNova.Storage.Supports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNova.Storage.EventStore
{
    /// <summary>
    /// Provides an API for a event stream consumer. Any consumer application
    /// can use the <see cref="RunAndBlock(Action{IEnumerable{EventBase}}, CancellationToken)"/> method
    /// to listen for incoming events into the stream. Behind the scene the API polls
    /// the commit trails for the relevant event stream.
    /// </summary>
    public class EventStreamConsumer
    {
        #region Core Implementation
        public virtual async Task RunAndBlock(
            Action<IEnumerable<EventBase>> onEventReceived, CancellationToken cancellationToken)
        {
            var commitLogs = await GetCommitLogsAsync().ConfigureAwait(false);
            var leaseStore = await GetLeaseStoreAsync().ConfigureAwait(false);

            do
            {
                var recentCommitId =
                    await commitLogs.GetCurrentCommitIdAsync()
                    .ConfigureAwait(false);
                var offsetCommitId = (await leaseStore.TryGetOffsetCommitIdAsync()
                            .ConfigureAwait(false)) ?? 0;
                if (cancellationToken.IsCancellationRequested) break;

                if (offsetCommitId < recentCommitId)
                {
                    offsetCommitId = await ConsumeEvents(
                        onEventReceived, commitLogs, leaseStore, recentCommitId, offsetCommitId);
                }

                await Task.Delay(new TimeSpan(0, 0, 1)).ConfigureAwait(false);
            } while (!cancellationToken.IsCancellationRequested);
        }

        private async Task<long> ConsumeEvents(
            Action<IEnumerable<EventBase>> onEventReceived,
            ICommitTrailStore commitLogs, ILeaseStore leaseStore, long recentCommitId, long offsetCommitId)
        {
            // we have commits to take care ...
            var logs = (await commitLogs
                .GetCommitsAsync(offsetCommitId, recentCommitId)
                .ConfigureAwait(false))
                .Where(log =>
                    log.StreamName.Equals(this._streamName, StringComparison.OrdinalIgnoreCase));

            if (logs.Any())
            {
                var stream = await GetStreamAsync(commitLogs).ConfigureAwait(false);
                var events = await stream.ReadEventsAsync(logs).ConfigureAwait(false);

                onEventReceived(events);

                await leaseStore.SetOffsetCommitIdAsync((offsetCommitId = recentCommitId));
            }

            return offsetCommitId;
        }
        #endregion        

        #region Supporting methods
        public virtual async Task InitAsync()
        {
            this._credentials = new StorageCredentials(
                (await this._configStore.GetAsync(StorageConstants.TableAccountName).ConfigureAwait(false)),
                (await this._configStore.GetAsync(StorageConstants.TableAccountKey).ConfigureAwait(false)));
        }


        protected virtual async Task<ICommitTrailStore> GetCommitLogsAsync()
        {
            var commitLogs = new CommitTrailStore(
                this._credentials, this._tenant, this._loggerFactory);

            await commitLogs.InitAsync().ConfigureAwait(false);

            return commitLogs;
        }

        protected virtual async Task<ILeaseStore> GetLeaseStoreAsync()
        {
            var leaseStore = new LeaseStore(
                this._credentials,
                this._tenant, this._streamName, this._leaseName,
                this._loggerFactory);

            await leaseStore.InitAsync().ConfigureAwait(false);

            return leaseStore;
        }

        protected virtual async Task<IEventStream> GetStreamAsync(ICommitTrailStore commitLogs)
        {
            var stream = new EventStream(
                this._credentials, this._tenant,
                this._streamName, commitLogs, this._loggerFactory);

            await stream.Init(false);
            return stream;
        }
        #endregion

        #region Scaffolding
        private StorageCredentials _credentials;
        private readonly ConfigStore _configStore;
        private Tenant _tenant;
        private ILoggerFactory _loggerFactory;
        private string _leaseName;
        private string _streamName;

        public EventStreamConsumer(
            ConfigStore configStore,
            Tenant tenant,
            string streamName,
            string leaseName,
            ILoggerFactory loggerFactory)
        {
            Ensure.ArgumentNotNull(configStore, nameof(configStore));
            Ensure.ArgumentNotNull(tenant, nameof(tenant));
            Ensure.ArgumentNotNull(loggerFactory, nameof(loggerFactory));

            Ensure.ArgumentNotNullOrWhiteSpace(streamName, nameof(streamName));
            Ensure.ArgumentNotNullOrWhiteSpace(leaseName, nameof(leaseName));

            this._configStore = configStore;
            this._tenant = tenant;
            this._leaseName = leaseName;
            this._streamName = streamName;
            this._loggerFactory = loggerFactory;
        }
        #endregion
    }
}
