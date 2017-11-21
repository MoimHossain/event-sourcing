

using SuperNova.Shared.Messaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNova.Shared.EventStore.Transactions
{
    /// <summary>
    /// An API model that enables aggregates 
    /// write business logics in a transactional code-flow.
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// Add an event into the current transaction context
        /// </summary>
        /// <param name="event">An event instance</param>
        void AddEvent(EventBase @event);

        /// <summary>
        /// Add multiple events with sequence into the current transaction context
        /// </summary>
        /// <param name="events">A collection of events</param>
        void AddEvents(IEnumerable<EventBase> events);

        /// <summary>
        /// Commits the events in a single Unit of data write operations
        /// </summary>
        /// <returns></returns>
        Task CommitAsync();
    }
}
