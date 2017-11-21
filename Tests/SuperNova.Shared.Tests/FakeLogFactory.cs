using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNova.Shared.Tests
{
    public class FakeLogFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
         
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FakeLogger();
        }

        public void Dispose()
        {
            
        }
    }
}
