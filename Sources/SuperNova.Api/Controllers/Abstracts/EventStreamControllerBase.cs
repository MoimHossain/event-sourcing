using SuperNova.Shared.DomainObjects;
using SuperNova.Shared.EventStore;
using SuperNova.Shared.Repositories;
using SuperNova.Shared.Supports;
using SuperNova.Storage.Supports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SuperNova.Api.Controllers
{
    public abstract class EventStreamControllerBase : Controller
    {
        private IEventStore eventStore;
        private ILoggerFactory logFactory;
        private IRepositoryFactory repositoryFactory;

        public EventStreamControllerBase(
            IRepositoryFactory repositoryFactory,
            IEventStore eventStore, ILoggerFactory logFactory)
        {
            Ensure.ArgumentNotNull(repositoryFactory, nameof(repositoryFactory));
            Ensure.ArgumentNotNull(eventStore, nameof(eventStore));
            Ensure.ArgumentNotNull(logFactory, nameof(logFactory));

            this.logFactory = logFactory;
            this.eventStore = eventStore;
            this.repositoryFactory = repositoryFactory;
        }


        public virtual async Task<TTableStore> CreateTableRepositoryAsync<TTableStore>(Tenant tenant) where TTableStore : IRepository
        {
            return await repositoryFactory.CreateTableRepositoryAsync<TTableStore>(tenant).ConfigureAwait(false);
        }

        public virtual async Task<TDocumentStore> CreateDocumentRepositoryAsync<TDocumentStore>(Tenant tenant) where TDocumentStore : IRepository
        {
            return await repositoryFactory.CreateDocumentRepositoryAsync<TDocumentStore>(tenant).ConfigureAwait(false);
        }

        protected virtual async Task ExecuteEditAsync<TAggregate>(
            Tenant tenant, string streamName, Guid aggregateId,
            Func<TAggregate, Task> work) where TAggregate : AggregateRoot, new()
        {
            await this.ExecuteEditAsync(
                tenant, streamName, aggregateId,
                () => new TAggregate(), 
                async (aggregate) =>
                {
                    await work((TAggregate)aggregate);
                }).ConfigureAwait(false);
        }

        protected virtual async Task ExecuteEditAsync(
            Tenant tenant, string streamName, Guid aggregateId,
            Func<AggregateRoot> factoryMethod,
            Func<AggregateRoot, Task> work)
        {
            var es = this.eventStore;
            var stream = await es.GetStreamAsync(tenant, streamName).ConfigureAwait(false);
            
            await es.ExecuteInTransactionAsync(
                tenant, streamName, aggregateId, async (tx) => {

                    var events = await stream.GetEventsForAggregate
                        (aggregateId).ConfigureAwait(false);

                    var aggregate = factoryMethod();
                    aggregate.LoadsFromHistory(events);
                    // Perform business operation here
                    await work(aggregate);

                    tx.AddEvents(aggregate.GetUncommittedChanges());
                    await Task.CompletedTask;
                }).ConfigureAwait(false);
        }

        protected virtual async Task ExecuteNewAsync(
            Tenant tenant, string streamName, Guid aggregateId,
            Func<Task<AggregateRoot>> work) 
        {
            var es = this.eventStore;

            await es.ExecuteInTransactionAsync(
                tenant, streamName, aggregateId, async (tx) => {
                    // Perform business operation here
                    var aggregate = await work();

                    tx.AddEvents(aggregate.GetUncommittedChanges());
                    await Task.CompletedTask;
                }).ConfigureAwait(false);
        }
    }
}
