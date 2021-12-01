namespace EdgeSecrets.CryptoProvider
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICryptoProvider
    {
        /// <summary>
        /// Encrypts data. 
        /// </summary>
        /// <param name="plaintext">Plain text value to be encrypted.</param>
        /// <param name="keyId">ID of the key to use</param>
        /// <returns>Encrypted the data encoded as base64.</returns>
        Task<string> EncryptAsync(string plaintext, string keyId, CancellationToken ct = default);

        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="ciphertext">Encoded data to be decrypted.</param>
        /// <param name="keyId">ID of the key to use</param>
        /// <returns>Decrypted data.</returns>
        Task<string> DecryptAsync(string ciphertext, string keyId, CancellationToken ct = default);
    }
}
