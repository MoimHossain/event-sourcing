using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNova.Storage.Supports
{
    public class KeysPair
    {
        public KeysPair(string partitionkey, string rowKey)
        {
            this.PartitionKey = partitionkey;
            this.RowKey = rowKey;
        }

        public string PartitionKey { get; private set; }
        public string RowKey { get; private set; }
    }
}
