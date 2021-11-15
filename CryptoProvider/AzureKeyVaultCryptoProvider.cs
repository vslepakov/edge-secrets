namespace EdgeSecrets.CryptoProvider
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Azure.Security.KeyVault.Keys.Cryptography;

    // TODO: start using symmetric keys with Managed HSM

    public class AzureKeyVaultCryptoProvider : ICryptoProvider
    {
        public async Task<string> EncryptAsync(string plaintext, string keyId, CancellationToken ct = default)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            var cryptographyClient = new CryptographyClient(new Uri(keyId), new EnvironmentCredential());
            var encryptResult = await cryptographyClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, plaintextBytes, ct);

            return Convert.ToBase64String(encryptResult.Ciphertext);
        }

        public async Task<string> DecryptAsync(string ciphertext, string keyId, CancellationToken ct = default)
        {
            var ciphertextBytes = Convert.FromBase64String(ciphertext);

            var cryptographyClient = new CryptographyClient(new Uri(keyId), new EnvironmentCredential());
            var decryptResult = await cryptographyClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, ciphertextBytes, ct);

            return Encoding.UTF8.GetString(decryptResult.Plaintext);
        }
    }
}
