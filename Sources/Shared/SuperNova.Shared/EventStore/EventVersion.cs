

using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNova.Shared.EventStore
{
    /// <summary>
    /// Represents an event version that is unique with in an event stream
    /// </summary>
    public class EventVersion
    {
        public static readonly EventVersion Empty = new EventVersion(-1, string.Empty);

        public EventVersion(long version, string etag)
        {
            this.Version = version;
            this.ETag = etag;
        }

        public long Version { get; set; }

        public string ETag { get; private set; }
    }
}
