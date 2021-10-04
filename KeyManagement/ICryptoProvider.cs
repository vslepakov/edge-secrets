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
        /// <param name="keyType">ECC or RSA.</param>
        /// <param name="keyId">KeyId of the Key to be used. Can be used to get the actual keyHandle.</param>
        /// This parameter is optional since not all providers support key selection.</param>
        /// <returns>Encrypted the data encoded as base64.</returns>
        Task<string> EncryptAsync(string plaintext, string keyId, KeyType keyType, CancellationToken ct = default);

        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="ciphertext">Encoded data to be decrypted.</param>
        /// <param name="keyType">EC or RSA.</param>
        /// /// <param name="keyId">KeyId of the Key to be used. Can be used to get the actual keyHandle.</param>
        /// This parameter is optional since not all providers support key selection</param>
        /// <returns>Decrypted data.</returns>
        Task<string> DecryptAsync(string ciphertext, string keyId, KeyType keyType, CancellationToken ct = default);
    }
}
