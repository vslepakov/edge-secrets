namespace EdgeSecrets.SecretManager
{
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public class SecretManagerClient
    {
        private ISecretStore _localSecretStore;
        private ISecretStore _remoteSecretStore;
        private ICryptoProvider _cryptoProvider;
        private KeyOptions _keyOptions;

        public SecretManagerClient(ICryptoProvider cryptoProvider, KeyOptions keyOptions, ISecretStore secretStore)
        {
            _localSecretStore = secretStore;
            _cryptoProvider = cryptoProvider;
            _keyOptions = keyOptions;
        }

        public SecretManagerClient(ICryptoProvider cryptoProvider, KeyOptions keyOptions, ISecretStore localSecretStore, ISecretStore remoteSecretStore)
        {
            _localSecretStore = localSecretStore;
            _remoteSecretStore = remoteSecretStore;
            _cryptoProvider = cryptoProvider;
            _keyOptions = keyOptions;
        }

        public async Task<string> GetSecretValueAsync(string name)
        {
            var secret = await _localSecretStore.GetSecretAsync(name);
            if (secret != null)
            {
                return await _cryptoProvider.DecryptAsync(secret.Value, _keyOptions);
            }
            return null;
        }

        public async Task SetSecretValueAsync(string name, string plainText)
        {
            string encryptedValue = await _cryptoProvider.EncryptAsync(plainText, _keyOptions);
            var secret = await _localSecretStore.GetSecretAsync(name);
            if (secret != null)
            {
                secret = secret with { Value = encryptedValue };
            }
            else
            {
                secret = new Secret() { Name = name, Value = encryptedValue };
            }
            await _localSecretStore.SetSecretAsync(name, secret);
        }
    }
}
