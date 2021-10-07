using System;
using System.Threading.Tasks;
using EdgeSecrets.KeyManagement;
using EdgeSecrets.Samples.SecretManager.Common;
using KeyManagement;

namespace EdgeSecrets.Samples.SecretManager.Common
{
    public class SecretManager
    {
        private ISecretStore _secretStore;
        private ICryptoProvider _cryptoProvider;
        private KeyManagementService _keyManager;

        public SecretManager(ICryptoProvider cryptoProvider, ISecretStore secretStore)
        {
            _secretStore = secretStore;
            _cryptoProvider = cryptoProvider;
            _keyManager = new KeyManagementService(new KeyOptions { KeyId = "KEY_ID", KeyType = KeyType.RSA }, _cryptoProvider);
        }

        public async Task<string> GetSecretAsync(string key)
        {
            string ciphertext = await _secretStore.GetSecretAsync(key);
            string decryptedValue = await _keyManager.DecryptAsync(ciphertext);
            return decryptedValue;
        }

        public async Task SetSecretAsync(string key, string plainText)
        {
            string encryptedValue = await _keyManager.EncryptAsync(plainText);
            await _secretStore.SetSecretAsync(key, encryptedValue);
        }
    }
}
