using KeyManagement;
using System;
using System.Threading.Tasks;

namespace EdgeSecrets.KeyManagement
{
    public class KeyManagementService : IKeyManagementService
    {
        private readonly ICryptoProvider _cryptoProvider;
        private readonly KeyOptions _keyOptions;

        public KeyManagementService(KeyOptions keyOptions, ICryptoProvider cryptoProvider)
        {
            _cryptoProvider = cryptoProvider;
            _keyOptions = keyOptions;
        }

        public async Task<string> DecryptAsync(string ciphertext)
        {
            return await _cryptoProvider.DecryptAsync(ciphertext, _keyOptions);
        }

        public async Task<string> EncryptAsync(string plaintext)
        {
            return await _cryptoProvider.EncryptAsync(plaintext, _keyOptions);
        }

        public Task ForgetMeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
