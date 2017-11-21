

using System;

namespace SuperNova.Shared.Messaging
{
    public abstract class EventBase : MessageBase
    {
        public Guid AggregateId { get; set; }
        public long Version { get; set; }
    }
}
