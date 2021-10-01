using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeSecrets.KeyManagement
{
    public class AzureKeyVaultCryptoProvider : ICryptoProvider
    {
        public async Task<string> DecryptAsync(string ciphertext, string keyId, KeyType keyType, CancellationToken ct = default)
        {
            var cryptographyClient = new CryptographyClient(new Uri(keyId), new EnvironmentCredential());

            var ciphertextBytes = Encoding.ASCII.GetBytes(ciphertext);
            var decryptResult = await cryptographyClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, ciphertextBytes);

            return Encoding.ASCII.GetString(decryptResult.Plaintext);
        }

        public async Task<string> EncryptAsync(string plaintext, string keyId, KeyType keyType, CancellationToken ct = default)
        {
            var cryptographyClient = new CryptographyClient(new Uri(keyId), new EnvironmentCredential());

            var plaintextBytes = Encoding.ASCII.GetBytes(plaintext);
            var encryptResult = await cryptographyClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, plaintextBytes);

            return Encoding.ASCII.GetString(encryptResult.Ciphertext);
        }
    }
}
