using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNova.Shared.Exceptions
{
    public class OptimisticConcurrencyException : Exception
    {
        public OptimisticConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {

        }

        public OptimisticConcurrencyException(string message) : this(message, null)
        {

        }

        public OptimisticConcurrencyException() : this("Optimistic concurrency occured.")
        {

        }
    }
}
