

using SuperNova.Shared.Repositories;
using SuperNova.Shared.Supports;
using SuperNova.Storage.Supports;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace SuperNova.Storage.Repositories
{
    /// <summary>
    /// An abstraction for all repository classes
    /// to ease operations with handy protected methods.
    /// </summary>
    public abstract class TableStoreBase : IRepository
    {
        private string _tableName;
        private CloudTable _table;
        private readonly ILogger _logger;
        private readonly StorageCredentials _credentials;

        protected TableStoreBase(
            string tableName,
            StorageCredentials credentials,
            ILoggerFactory factory,
            bool createTableIfNotExists)
        {
            Ensure.ArgumentNotNull(factory, nameof(factory));
            Ensure.ArgumentNotNull(credentials, nameof(credentials));
            Ensure.ArgumentNotNullOrWhiteSpace(tableName, nameof(tableName));

            //TODO: Bedrijfsnaam moet uit het token komen
            var bedrijfsNaam = "defaultbedijf";

            _tableName = tableName + bedrijfsNaam;
            _credentials = credentials;
            _logger = factory.CreateLogger(GetType().FullName);
        }

        public virtual async Task<bool> Init()
        {            
            _table= await TableExtensions
                .CreateTableClientAsync(
                    _credentials, _tableName, true, _logger)
                    .ConfigureAwait(false);

            return _table != null;
        }
        
        protected virtual CloudTable Table
        {
            get
            {
                return _table;
            }
        }

        protected ILogger Logger
        {
            get
            {
                return _logger;
            }
        }
    }
}
