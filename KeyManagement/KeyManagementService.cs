using System;
using System.Threading.Tasks;

namespace EdgeSecrets.KeyManagement
{
    public class KeyManagementService : IKeyManagementService
    {
        // TODO: hard coded for now. Device how to handle KEY IDs
        private const string KEY_ID = "https://keyvault-ca-2.vault.azure.net/keys/kms-key/84e7576868ff452b918ae5eeb05cf2e0";
        private readonly ICryptoProvider _cryptoProvider;

        public KeyManagementService(ICryptoProvider cryptoProvider)
        {
            _cryptoProvider = cryptoProvider;
        }

        public async Task<string> DecryptAsync(string ciphertext)
        {
            return await _cryptoProvider.DecryptAsync(ciphertext, KEY_ID, KeyType.RSA);
        }

        public async Task<string> EncryptAsync(string plaintext)
        {
            return await _cryptoProvider.EncryptAsync(plaintext, KEY_ID, KeyType.RSA);
        }

        public Task ForgetMeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
