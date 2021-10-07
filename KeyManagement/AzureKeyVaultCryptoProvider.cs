using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
using KeyManagement;
using KeyManagement.Exceptions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeSecrets.KeyManagement
{
    public class AzureKeyVaultCryptoProvider : ICryptoProvider
    {
        private const int RSA_OAEP_PADDING_SIZE_IN_BYTES = 42;

        public Task<string> DecryptAsync(string ciphertext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            return keyOptions.KeyType switch
            {
                KeyType.RSA => RsaDecryptAsync(ciphertext, keyOptions, ct),
                KeyType.ECC => throw new NotImplementedException(),
                KeyType.Symmetric => throw new NotImplementedException(),
                _ => throw new ArgumentException($"{keyOptions.KeyType} is not supported by this provider"),
            };
        }

        public Task<string> EncryptAsync(string plaintext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            return keyOptions.KeyType switch
            {
                KeyType.RSA => RsaEncryptAsync(plaintext, keyOptions, ct),
                KeyType.ECC => throw new NotImplementedException(),
                KeyType.Symmetric => throw new NotImplementedException(),
                _ => throw new ArgumentException($"{keyOptions.KeyType} is not supported by this provider"),
            };
        }

        private async Task<string> RsaDecryptAsync(string ciphertext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            var ciphertextBytes = Convert.FromBase64String(ciphertext);

            if (ciphertextBytes.Length > keyOptions.KeySize / 8)
            {
                throw new DataTooLargeException($"Data too large to decrypt using RSA-OAEP with the key size {keyOptions.KeySize}");
            }

            var cryptographyClient = new CryptographyClient(new Uri(keyOptions.KeyId), new EnvironmentCredential());
            var decryptResult = await cryptographyClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, ciphertextBytes, ct);

            return Encoding.UTF8.GetString(decryptResult.Plaintext);
        }

        private async Task<string> RsaEncryptAsync(string plaintext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            if (plaintextBytes.Length + RSA_OAEP_PADDING_SIZE_IN_BYTES > keyOptions.KeySize / 8)
            {
                throw new DataTooLargeException($"Data too large to encrypt using RSA-OAEP with the key size {keyOptions.KeySize}");
            }

            var cryptographyClient = new CryptographyClient(new Uri(keyOptions.KeyId), new EnvironmentCredential());
            var encryptResult = await cryptographyClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, plaintextBytes, ct);

            return Convert.ToBase64String(encryptResult.Ciphertext);
        }
    }
}
