
using SuperNova.Shared.DomainObjects;
using SuperNova.Shared.EventStore.Transactions;

using System.Threading.Tasks;

namespace SuperNova.Shared.EventStore
{
    // The interface for an event store. 
    // Stores are used to discover event streams and get a handle 
    // of event streams.
    public interface IEventStore
    {
        Task<IEventStream> GetStreamAsync(Tenant tenant, string streamName);
    }
}
