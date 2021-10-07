using System;
using System.Threading.Tasks;

namespace EdgeSecrets.KeyManagement
{
    public class KeyManagementService : IKeyManagementService
    {
        private readonly ICryptoProvider _cryptoProvider;
        private readonly string _keyId;
        private readonly KeyType _keyType;

        public KeyManagementService(string keyId, KeyType keyType, ICryptoProvider cryptoProvider)
        {
            _cryptoProvider = cryptoProvider;
            _keyId = keyId;
            _keyType = keyType;
        }

        public async Task<string> DecryptAsync(string ciphertext)
        {
            return await _cryptoProvider.DecryptAsync(ciphertext, _keyId, _keyType);
        }

        public async Task<string> EncryptAsync(string plaintext)
        {
            return await _cryptoProvider.EncryptAsync(plaintext, _keyId, _keyType);
        }

        public Task ForgetMeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
