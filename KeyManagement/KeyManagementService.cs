using System;
using System.Threading.Tasks;

namespace EdgeSecrets.KeyManagement
{
    public class KeyManagementService : IKeyManagementService
    {
        private readonly ICryptoProvider _cryptoProvider;

        public KeyManagementService(ICryptoProvider cryptoProvider)
        {
            _cryptoProvider = cryptoProvider;
        }

        public async Task<string> DecryptAsync(string ciphertext)
        {
            return await _cryptoProvider.DecryptAsync(ciphertext, "key", KeyType.RSA);
        }

        public async Task<string> EncryptAsync(string plaintext)
        {
            return await _cryptoProvider.EncryptAsync(plaintext, "key", KeyType.RSA);
        }

        public Task ForgetMeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
