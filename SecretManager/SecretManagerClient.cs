namespace EdgeSecrets.SecretManager
{
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public class SecretManagerClient
    {
        private ISecretStore _secretStore;

        public SecretManagerClient(ICryptoProvider cryptoProvider, KeyOptions keyOptions, ISecretStore secretStore)
        {
            _secretStore = secretStore;
        }

        public async Task<string> GetSecretValueAsync(string name, CancellationToken cancellationToken = default)
        {
            var secret = await _secretStore.GetSecretAsync(name, cancellationToken);
            if (secret != null)
            {
                return secret.Value;
            }
            return null;
        }

        public async Task SetSecretValueAsync(string name, string plainText, CancellationToken cancellationToken = default)
        {
            string encryptedValue = plainText;
            var secret = await _secretStore.GetSecretAsync(name, cancellationToken);
            if (secret != null)
            {
                secret = secret with { Value = encryptedValue };
            }
            else
            {
                secret = new Secret() { Name = name, Value = encryptedValue };
            }
            await _secretStore.SetSecretAsync(name, secret, cancellationToken);
        }
    }
}
