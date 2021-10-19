namespace Tests
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public class TestCryptoProvider : ICryptoProvider
    {
        public Task<string> DecryptAsync(string ciphertext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            // Simulate decrypt by reverting the string back to original value
            string plaintext = new string(ciphertext.ToCharArray().Reverse().ToArray());
            return Task.FromResult(plaintext);
        }

        public Task<string> EncryptAsync(string plaintext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            // Simulate encrypt by reverting the string
            string ciphertext = new string(plaintext.ToCharArray().Reverse().ToArray());
            return Task.FromResult(ciphertext);
        }
    }
}
