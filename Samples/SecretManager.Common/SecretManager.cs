using System;
using System.Threading.Tasks;
using EdgeSecrets.KeyManagement;
using EdgeSecrets.Samples.SecretManager.Common;

namespace EdgeSecrets.Samples.SecretManager.Common
{
    public class SecretManager
    {
        private ISecretStore _secretStore;
        private ICryptoProvider _crypteProvider;
        private KeyManagementService _keyManager;

        public SecretManager(ICryptoProvider crypteProvider, ISecretStore secretStore)
        {
            _secretStore = secretStore;
            _crypteProvider = crypteProvider;
            _keyManager = new KeyManagementService(_crypteProvider);
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
