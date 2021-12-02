namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class SecretManagerClient
    {
        public ISecretStore? SecretStore { get; set; }

        public SecretManagerClient(ISecretStore? secretStore = null)
        {
            SecretStore = secretStore;
        }

        public async Task ClearCacheAsync(CancellationToken cancellationToken = default)
        {
            if (SecretStore != null)
            {
                await SecretStore.ClearCacheAsync(cancellationToken);
            }
        }

        public async Task<Secret?> GetSecretAsync(string name, string? version, DateTime? date, CancellationToken cancellationToken = default)
        {
            if (SecretStore != null)
            {
                return await SecretStore.RetrieveSecretAsync(name, version, date, cancellationToken);
            }
            return null;
        }

        public async Task<string?> GetSecretValueAsync(string name, string? version, DateTime? date, CancellationToken cancellationToken = default)
        {
            if (SecretStore != null)
            {
                var secret = await SecretStore.RetrieveSecretAsync(name, version, date, cancellationToken);
                return secret?.Value;
            }
            return null;
        }

        public async Task<SecretList?> RetrieveSecretListAsync(IList<string> secretNames, CancellationToken cancellationToken = default)
        {
            if (SecretStore != null)
            {
                List<Secret?>? secrets = new();
                foreach (var secretName in secretNames)
                {
                    secrets.Add(new Secret(secretName));
                }
                return await SecretStore.RetrieveSecretListAsync(secrets, cancellationToken);
            }
            return null;
        }

        public async Task SetSecretValueAsync(string name, string plainText, string? version = null, CancellationToken cancellationToken = default)
        {
            if (SecretStore != null)
            {
                Secret? secret = await SecretStore.RetrieveSecretAsync(name, version, DateTime.UtcNow, cancellationToken);
                if (secret != null)
                {
                    secret = secret with { Value = plainText };
                }
                else
                {
                    secret = new Secret(name, plainText);
                }
                await SecretStore.StoreSecretAsync(secret, cancellationToken);
            }
        }
    }
}
