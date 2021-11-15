namespace EdgeSecrets.SecretManager
{
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public class SecretManagerClient
    {
        private ISecretStore _localSecretStore;
        private ISecretStore _remoteSecretStore;
        private ICryptoProvider _cryptoProvider;
        private string _keyId;

        public SecretManagerClient(ICryptoProvider cryptoProvider, string keyId, ISecretStore secretStore)
        {
            _localSecretStore = secretStore;
            _cryptoProvider = cryptoProvider;
            _keyId = keyId;
        }

        public SecretManagerClient(ICryptoProvider cryptoProvider, string keyId, ISecretStore localSecretStore, ISecretStore remoteSecretStore) 
            : this(cryptoProvider, keyId, localSecretStore)
        {
            _remoteSecretStore = remoteSecretStore;
        }

        public async Task<string> GetSecretValueAsync(string name)
        {
            var secret = await _localSecretStore.GetSecretAsync(name);
            if (secret != null)
            {
                return await _cryptoProvider.DecryptAsync(secret.Value, _keyId);
            }
            return null;
        }

        public async Task SetSecretValueAsync(string name, string plainText)
        {
            string encryptedValue = await _cryptoProvider.EncryptAsync(plainText, _keyId);
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
