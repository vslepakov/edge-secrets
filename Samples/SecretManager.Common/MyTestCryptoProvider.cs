using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdgeSecrets.KeyManagement;

namespace EdgeSecrets.Samples.SecretManager.Common
{
    public class MyTestCryptoProvider : ICryptoProvider
    {
        public Task<string> DecryptAsync(string ciphertext, string keyId, KeyType keyType, CancellationToken ct = default)
        {
            // Simulate decrypt by reverting the string back to original value
            string plaintext = new string(ciphertext.ToCharArray().Reverse().ToArray());
            return Task.FromResult(plaintext);
        }

        public Task<string> EncryptAsync(string plaintext, string keyId, KeyType keyType, CancellationToken ct = default)
        {
            // Simulate encrypt by reverting the string
            string ciphertext = new string(plaintext.ToCharArray().Reverse().ToArray());
            return Task.FromResult(ciphertext);
        }
    }
}
