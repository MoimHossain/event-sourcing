

using Microsoft.Extensions.Logging;
using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNova.Shared.Supports
{
    public static class EnvironmentSupport
    {
        public static void Dump(ILogger logger)
        {
            var keys = new List<string> { "ClientID", "ClientSecret", "VaultBaseUri" };

            foreach (var key in keys)
            {
                Console.WriteLine(string.Format("{0} = '{1}'", key, Environment.GetEnvironmentVariable(key)));
            }
        }

        public static bool HasFaults => !Environment.GetEnvironmentVariables()
            .Keys.OfType<string>().Contains("ClientSecret");

        public static string ClientID => Environment.GetEnvironmentVariable("ClientID");

        public static string ClientSecret => Environment.GetEnvironmentVariable("ClientSecret");

        public static Uri VaultBaseUri => new Uri(Environment.GetEnvironmentVariable("VaultBaseUri"));
    }
}
