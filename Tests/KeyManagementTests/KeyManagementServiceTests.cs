using System.Threading.Tasks;
using EdgeSecrets.KeyManagement;
using Xunit;

namespace Tests
{
    public class KeyManagementServiceTests
    {
        [Fact]
        public async Task Encrypt_And_Decrypt_Data_Using_Test_Provider_Success()
        {
            // Arrange
            const string PLAINTEXT = "Hello World";

            var akvProvider = new TestCryptoProvider();
            var kms = new KeyManagementService(new KeyOptions(), akvProvider);

            // Act
            var cipherText = await kms.EncryptAsync(PLAINTEXT);
            var plaintext = await kms.DecryptAsync(cipherText);

            // Assert
            Assert.False(string.IsNullOrEmpty(cipherText));
            Assert.Equal(PLAINTEXT, plaintext);
        }

        [Fact]
        public async Task Encrypt_And_Decrypt_Data_Using_AzureKeyVault_Provider_Success()
        {
            // Arrange
            const string PLAINTEXT = "Hello World";
            //const string KEY_ID = "https://keyvault-ca-2.vault.azure.net/keys/kms-key/84e7576868ff452b918ae5eeb05cf2e0";
            const string KEY_ID = "https://mvsdev3kv.vault.azure.net/keys/kms-key/2da6c35f4fe34496a5078b9a9983f042";
            

            var akvProvider = new AzureKeyVaultCryptoProvider();
            var kms = new KeyManagementService(new KeyOptions 
            {
                KeyId = KEY_ID, 
                KeyType = KeyType.RSA,
                KeySize = 2048
            }, akvProvider);

            // Act
            var cipherText = await kms.EncryptAsync(PLAINTEXT);
            var plaintext = await kms.DecryptAsync(cipherText);

            // Assert
            Assert.False(string.IsNullOrEmpty(cipherText));
            Assert.Equal(PLAINTEXT, plaintext);
        }
    }
}
