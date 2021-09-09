using System;
using System.Threading.Tasks;

namespace KeyManagementService
{
    public class KeyManagementService : IKeyManagementService
    {
        private readonly ICryptoProvider _cryptoProvider;

        public KeyManagementService(ICryptoProvider cryptoProvider)
        {
            _cryptoProvider = cryptoProvider;
        }

        public Task<string> DecryptAsync(string ciphertext)
        {
            throw new NotImplementedException();
        }

        public Task<string> EncryptAsync(string plaintext)
        {
            throw new NotImplementedException();
        }

        public Task ForgetMeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
