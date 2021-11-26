namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class SecretManagerClient
    {
        private readonly int Timeout = 15000;
        private readonly ISecretStore _secretStore;

        public SecretManagerClient(ISecretStore secretStore)
        {
            _secretStore = secretStore;
        }

        public async Task ClearCacheAsync(CancellationToken cancellationToken = default)
        {
            await _secretStore.ClearCacheAsync(cancellationToken);
        }

        public async Task<Secret?> GetSecretAsync(string name, string? version, DateTime? date, CancellationToken cancellationToken = default)
        {
            return await _secretStore.RetrieveSecretAsync(name, version, date, cancellationToken);
        }

        public async Task<string?> GetSecretValueAsync(string name, string? version, DateTime? date, CancellationToken cancellationToken = default)
        {
            using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                linkedTokenSource.CancelAfter(Timeout);
                try
                {
                    var secret = await _secretStore.RetrieveSecretAsync(name, version, date, linkedTokenSource.Token);
                    return secret?.Value;
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("GetSecretValueAsync cancelled: timed out.");
                }
            }
            return null;
        }

        public async Task<SecretList?> RetrieveSecretsAsync(IList<string> secretNames, CancellationToken cancellationToken = default)
        {
            List<Secret?>? secrets = new();
            foreach (var secretName in secretNames)
            {
                secrets.Add(new Secret(secretName));
            }
            return await _secretStore.RetrieveSecretsAsync(secrets, cancellationToken);
        }

        public async Task SetSecretValueAsync(string name, string plainText, string? version = null, CancellationToken cancellationToken = default)
        {
            Secret? secret = await _secretStore.RetrieveSecretAsync(name, version, DateTime.UtcNow, cancellationToken);
            if (secret != null)
            {
                secret = secret with { Value = plainText };
            }
            else
            {
                secret = new Secret(name, plainText);
            }
            await _secretStore.StoreSecretAsync(secret, cancellationToken);
        }
    }
}
