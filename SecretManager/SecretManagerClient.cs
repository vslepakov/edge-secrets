namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class SecretManagerClient
    {
        private ISecretStore _secretStore;

        public SecretManagerClient(ISecretStore secretStore)
        {
            _secretStore = secretStore;
        }

        public async Task<string> GetSecretValueAsync(string name, DateTime date, CancellationToken cancellationToken = default)
        {
            var secret = await _secretStore.GetSecretAsync(name, date, cancellationToken);
            if (secret != null)
            {
                return secret.Value;
            }
            return null;
        }

        public async Task SetSecretValueAsync(string name, string plainText, CancellationToken cancellationToken = default)
        {
            var secret = await _secretStore.GetSecretAsync(name, DateTime.UtcNow, cancellationToken);
            if (secret != null)
            {
                secret = secret with { Value = plainText };
            }
            else
            {
                secret = new Secret() { Name = name, Value = plainText };
            }
            await _secretStore.SetSecretAsync(secret, cancellationToken);
        }
    }
}
