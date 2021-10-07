using KeyManagement;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeSecrets.KeyManagement
{
    public class WorkloadApiCryptoProvider : ICryptoProvider
    {
        public Task<string> DecryptAsync(string ciphertext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> EncryptAsync(string plaintext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
