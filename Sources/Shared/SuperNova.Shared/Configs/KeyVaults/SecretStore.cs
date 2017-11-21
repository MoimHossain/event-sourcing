

using SuperNova.Shared.Supports;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace SuperNova.Shared.Configs.KeyVaults
{
    public sealed class SecretStore
    {
        private readonly ILogger logger;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly Uri vaultBaseUrl;
        public SecretStore(
            string clientId, string clientSecret, 
            Uri vaultBaseUrl, ILoggerFactory loggerFactory)
        {
            Ensure.ArgumentNotNull(loggerFactory, nameof(loggerFactory));
            Ensure.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId));
            Ensure.ArgumentNotNullOrWhiteSpace(clientSecret, nameof(clientSecret));
            Ensure.ArgumentNotNull(vaultBaseUrl, nameof(vaultBaseUrl));

            this.logger = loggerFactory.CreateLogger<SecretStore>();
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.vaultBaseUrl = vaultBaseUrl;
        }

        public async Task<string> GetConfigValue(string secretName)
        {
            using (var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken)))
            {
                var secretBundle = await kv.GetSecretAsync(vaultBaseUrl.ToString(), secretName);
                return secretBundle.Value;
            }
        }

        public async Task<string> GetConfigValue(Uri secretIdentifier)
        {
            using (var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken)))
            {
                var secretBundle = await kv.GetSecretAsync(secretIdentifier.ToString());
                return secretBundle.Value;
            }
        }

        private async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            var clientCred = new ClientCredential(clientId, clientSecret);

            var result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
            {
                var message = "Failed to obtain the JWT token from Active Directory (Key Vault).";

                logger.LogCritical(message);
                throw new InvalidOperationException(message);
            }
            return result.AccessToken;
        }
    }
}
