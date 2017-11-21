
using SuperNova.Shared.Configs.KeyVaults;
using SuperNova.Shared.Supports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNova.Shared.Configs
{
    public class ConfigStore
    {
        private ILogger logger;        
        private readonly SecretStore _secretStore;
        private readonly Dictionary<string, string> _keyValues;

        public ConfigStore(
            bool isDevelopmentEnvironment,
            Func<string, string> confReader, 
            ILoggerFactory logFactory)
        {
            Ensure.ArgumentNotNull(logFactory, nameof(logFactory));
            this.logger = logFactory.CreateLogger<ConfigStore>();

            EnvironmentSupport.Dump(this.logger);

            if (isDevelopmentEnvironment)
            {
                Ensure.ArgumentNotNull(confReader, nameof(confReader));
                var message = "API is booting as dev environment.";
                // For now, just warning, in future we will fail in this scenario
                // Environment.FailFast(message);
                logger.LogCritical(message);

                _keyValues = new Dictionary<string, string>
                {
                    { "storage-table-account-name", confReader("StorageConfig:AccountName") },
                    { "storage-table-account-key", confReader("StorageConfig:AccountKey") },
                    { "storage-document-account-endpoint", confReader("StorageConfig:DocumentEndpoint") },
                    { "storage-document-account-key", confReader("StorageConfig:DocumentKey") }
                };
            }
            else
            {
                Console.WriteLine("Creating secret store...");
                _secretStore = new SecretStore(
                    EnvironmentSupport.ClientID,
                    EnvironmentSupport.ClientSecret,
                    EnvironmentSupport.VaultBaseUri,
                    logFactory);
            }
        }

        public async Task<string> GetAsync(string key) => await SafeGetKeyValueAsync(key);

        protected virtual async Task<string> SafeGetKeyValueAsync(string key)
        {
            return await (_secretStore != null 
                ? _secretStore.GetConfigValue(key) 
                : Task.FromResult(_keyValues[key]));
        }     
    }
}
