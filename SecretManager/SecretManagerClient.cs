namespace EdgeSecrets.SecretManager
{
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public class SecretManagerClient
    {
        private ISecretStore _secretStore;
        private ICryptoProvider _cryptoProvider;
        private KeyOptions _keyOptions;

        public SecretManagerClient(ICryptoProvider cryptoProvider, KeyOptions keyOptions, ISecretStore secretStore)
        {
            _secretStore = secretStore;
            _cryptoProvider = cryptoProvider;
            _keyOptions = keyOptions;
        }

        public async Task<string> GetSecretAsync(string key)
        {
            string ciphertext = await _secretStore.GetSecretAsync(key);
            string decryptedValue = await _cryptoProvider.DecryptAsync(ciphertext, _keyOptions);
            return decryptedValue;
        }

        public async Task SetSecretAsync(string key, string plainText)
        {
            string encryptedValue = await _cryptoProvider.EncryptAsync(plainText, _keyOptions);
            await _secretStore.SetSecretAsync(key, encryptedValue);
        }
    }
}
