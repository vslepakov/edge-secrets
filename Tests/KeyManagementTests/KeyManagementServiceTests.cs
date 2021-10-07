using EdgeSecrets.KeyManagement;
using KeyManagement;
using System.Threading.Tasks;
using Xunit;

namespace Tests.KeyManagementTests
{
    public class KeyManagementServiceTests
    {
        [Fact]
        public async Task Encrypt_And_Decrypt_Data_Using_AzureKeyVault_Provider_Success()
        {
            // Arrange
            const string PLAINTEXT = "Hello World";
            const string KEY_ID = "https://keyvault-ca-2.vault.azure.net/keys/kms-key/84e7576868ff452b918ae5eeb05cf2e0";

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
