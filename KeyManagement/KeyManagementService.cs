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
            throw new NotImplementedException();
        }

        public async Task<string> EncryptAsync(string plaintext)
        {
            throw new NotImplementedException();
        }

        public Task ForgetMeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
