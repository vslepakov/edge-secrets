using KeyManagement;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeSecrets.KeyManagement
{
    public interface ICryptoProvider
    {
        /// <summary>
        /// Encrypts data. 
        /// </summary>
        /// <param name="plaintext">Plain text value to be encrypted.</param>
        /// <param name="keyOptions">Key options describing the key</param>
        /// <returns>Encrypted the data encoded as base64.</returns>
        Task<string> EncryptAsync(string plaintext, KeyOptions keyOptions, CancellationToken ct = default);

        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="ciphertext">Encoded data to be decrypted.</param>
        /// <param name="keyOptions">Key options describing the key</param>
        /// <returns>Decrypted data.</returns>
        Task<string> DecryptAsync(string ciphertext, KeyOptions keyOptions, CancellationToken ct = default);
    }
}
