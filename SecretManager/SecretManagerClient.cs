namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class SecretManagerClient
    {
        private readonly ISecretStore _secretStore;

        public SecretManagerClient(ISecretStore secretStore)
        {
            _secretStore = secretStore;
        }

        public async Task ClearCacheAsync(CancellationToken cancellationToken = default)
        {
            await _secretStore?.ClearCacheAsync(cancellationToken);
        }

        public async Task<Secret> GetSecretAsync(string name, DateTime date, CancellationToken cancellationToken = default)
        {
            return await _secretStore?.RetrieveSecretAsync(name, date, cancellationToken);
        }

        public async Task<string> GetSecretValueAsync(string name, DateTime date, CancellationToken cancellationToken = default)
        {
            var secret = await _secretStore?.RetrieveSecretAsync(name, date, cancellationToken);
            return secret?.Value;
        }

        public async Task<SecretList> RetrieveSecretsAsync(IList<string> secretNames, CancellationToken cancellationToken = default)
        {
            return await _secretStore?.RetrieveSecretsAsync(secretNames, cancellationToken);
        }

        public async Task SetSecretValueAsync(string name, string plainText, CancellationToken cancellationToken = default)
        {
            var secret = await _secretStore?.RetrieveSecretAsync(name, DateTime.UtcNow, cancellationToken);
            if (secret != null)
            {
                secret = secret with { Value = plainText };
            }
            else
            {
                secret = new Secret() { Name = name, Value = plainText };
            }
            await _secretStore?.StoreSecretAsync(secret, cancellationToken);
        }
    }
}
