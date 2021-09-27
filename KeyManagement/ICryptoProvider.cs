using System.Threading;
using System.Threading.Tasks;

namespace EdgeSecrets.KeyManagement
{
    public interface ICryptoProvider
    {
        /// <summary>
        /// Encrypts data. 
        /// </summary>
        /// <param name="plaintext">base64 encoded plain text value to be encrypted.</param>
        /// <param name="keyType">ECC or RSA.</param>
        /// <param name="keyId">KeyId of the Key to be used. Can be used to get the actual keyHandle.</param>
        /// This parameter is optional since not all providers support key selection.</param>
        /// <returns>The encrypted form of the data encoded in base 64.</returns>
        Task<string> EncryptAsync(string plaintext, string keyId, KeyType keyType, CancellationToken ct = default);

        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="ciphertext">base64 encoded data to be decrypted.</param>
        /// <param name="keyType">EC or RSA.</param>
        /// /// <param name="keyId">KeyId of the Key to be used. Can be used to get the actual keyHandle.</param>
        /// This parameter is optional since not all providers support key selection</param>
        /// <returns>The decrypted form of the data encoded in base 64.</returns>
        Task<string> DecryptAsync(string ciphertext, string keyId, KeyType keyType, CancellationToken ct = default);
    }
}
