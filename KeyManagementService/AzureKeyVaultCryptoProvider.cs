using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeyManagementService
{
    public class AzureKeyVaultCryptoProvider : ICryptoProvider
    {
        public Task<string> DecryptAsync(string ciphertext, string keyId, KeyType keyType, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> EncryptAsync(string plaintext, string keyId, KeyType keyType, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
