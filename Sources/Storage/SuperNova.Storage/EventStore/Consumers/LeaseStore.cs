using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using SuperNova.Shared.DomainObjects;
using SuperNova.Shared.EventStore;
using SuperNova.Shared.Supports;
using SuperNova.Storage.Supports;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNova.Storage.EventStore
{
    /// <summary>
    /// An event stream (tenant and stream event type) can be consumed
    /// my any consumer as change feed processor. Multiple consumers can 
    /// read changes from the same event stream. 
    /// Each consumer manages their own lease (a pointer to the stream), that
    /// allows them to catch up in the event of a consumer failure. Cleaning up
    /// the lease pointer allows them to start form the beginning of the stream.
    /// This class enables that pointer management for a given stream consumer.
    /// </summary>
    public class LeaseStore : ILeaseStore
    {
        private StorageCredentials _credentials;
        private Tenant _tenant;
        private string _streamName;
        private string _leaseName;
        private ILogger<LeaseStore> _logger;
        private CloudTable _table;

        public LeaseStore(
            StorageCredentials credentials,
            Tenant tenant,
            string streamName, string leaseName,
            ILoggerFactory factory)
        {
            Ensure.ArgumentNotNull(credentials, nameof(credentials));
            Ensure.ArgumentNotNull(tenant, nameof(tenant));
            Ensure.ArgumentNotNull(factory, nameof(factory));
            Ensure.ArgumentNotNullOrWhiteSpace(streamName, nameof(streamName));
            Ensure.ArgumentNotNullOrWhiteSpace(leaseName, nameof(leaseName));

            this._credentials = credentials;
            this._tenant = tenant;
            this._streamName = streamName;
            this._leaseName = leaseName;
            this._logger = factory.CreateLogger<LeaseStore>();
        }

        public virtual async Task<bool> InitAsync()
        {
            _table = await TableExtensions
                .CreateTableClientAsync(
                    _credentials, TableName, true, _logger)
                    .ConfigureAwait(false);

            return _table != null;
        }

        protected virtual string TableName
        { get => $"{StorageConstants.Tables.LeaseTable}{this._tenant.TenantId.ToSafeStorageKey()}"; }

        public virtual async Task SetOffsetCommitIdAsync(long offsetCommitId)
        {
            var dte = new DynamicTableEntity(this._streamName, this._leaseName);
            dte.Properties = new Dictionary<string, EntityProperty>
            {
                { StorageConstants.Columns.OffsetCommitId, EntityProperty.GeneratePropertyForLong(offsetCommitId) }
            };

            await this._table.ExecuteAsync(TableOperation.InsertOrReplace(dte)).ConfigureAwait(false);
        }

        public virtual async Task<long?> TryGetOffsetCommitIdAsync()
        {
            var offsetCommitId = default(long?);

            var dte = await this._table.RetrieveAsync(
                this._streamName, this._leaseName, true).ConfigureAwait(false);

            if (dte != null)
            {
                offsetCommitId = dte.Properties[StorageConstants.Columns.OffsetCommitId].Int64Value;
            }
            return offsetCommitId;
        }
    }
}
