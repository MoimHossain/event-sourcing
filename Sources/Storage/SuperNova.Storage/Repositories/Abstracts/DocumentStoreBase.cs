
using SuperNova.Shared.Configs;
using SuperNova.Shared.Repositories;
using SuperNova.Shared.Supports;
using SuperNova.Storage.Supports;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SuperNova.Storage.Repositories
{
    public abstract class DocumentStoreBase : IRepository
    {
        private ConfigStore _configStore;
        private DocumentClient _documentClient;     
        private bool _initialized = false;

        protected DocumentStoreBase(ConfigStore configStore, ILoggerFactory logFactory)
        {
            Ensure.ArgumentNotNull(configStore, nameof(configStore));

            this._configStore = configStore;
        }

        public virtual async Task<bool> Init()
        {
            var endPoint = (await _configStore.GetAsync(StorageConstants.DocumentEndpoint).ConfigureAwait(false));
            var key = (await _configStore.GetAsync(StorageConstants.DocumentKey).ConfigureAwait(false));

            _documentClient = new DocumentClient(new Uri(endPoint), key);
            
            await _documentClient.CreateDatabaseIfNotExistsAsync(this.Database).ConfigureAwait(false);
            await _documentClient.CreateCollectionIfNotExistsAsync(this.Database, this.Collection).ConfigureAwait(false);
            
            _initialized = true;
            return _initialized;
        }

        protected virtual bool Initialized() => _initialized;
        protected virtual DocumentClient Client { get => this._documentClient; }

        protected abstract string Database { get; }

        protected abstract string Collection { get; }


        public virtual async Task<TDocument> GetAsync<TDocument>(string id)
        {
            try
            {
                var document = await _documentClient.ReadDocumentAsync
                    (UriFactory.CreateDocumentUri(this.Database, this.Collection, id))
                    .ConfigureAwait(false);
                return (TDocument)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return default(TDocument);
                }
                else
                {
                    throw;
                }
            }
        }

        public virtual async Task<IEnumerable<TDocument>> QueryAsync<TDocument>
            (Expression<Func<TDocument, bool>> predicate)
        {
            var query = _documentClient.CreateDocumentQuery<TDocument>(
                UriFactory.CreateDocumentCollectionUri(this.Database, this.Collection),
                new FeedOptions { MaxItemCount = 100 })
                .Where(predicate)
                .AsDocumentQuery();

            var results = new List<TDocument>();
            while (query.HasMoreResults)
            {
                results.AddRange((await query.ExecuteNextAsync<TDocument>().ConfigureAwait(false)));
            }
            return results;
        }

        public virtual async Task<Document> CreateAsync<TDocument>(TDocument document)
        {
            return await _documentClient.CreateDocumentAsync(
                UriFactory.CreateDocumentCollectionUri(this.Database, this.Collection), document)
                .ConfigureAwait(false);
        }

        public virtual async Task<Document> ReplaceAsync<TDocument>(string id, TDocument document)
        {
            return await _documentClient.ReplaceDocumentAsync(
                UriFactory.CreateDocumentUri(this.Database, this.Collection, id), document)
                .ConfigureAwait(false);
        }

        public virtual async Task DeleteAsync(string id)
        {
            await _documentClient.DeleteDocumentAsync(
                UriFactory.CreateDocumentUri(this.Database, this.Collection, id))
                .ConfigureAwait(false);
        }
    }
}
