using EdgeSecrets.KeyManagement;
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
            var akvProvider = new AzureKeyVaultCryptoProvider();
            var kms = new KeyManagementService(akvProvider);

            // Act
            var cipherText = await kms.EncryptAsync(PLAINTEXT);
            var plaintext = await kms.DecryptAsync(cipherText);

            // Assert
            Assert.False(string.IsNullOrEmpty(cipherText));
            Assert.Equal(PLAINTEXT, plaintext);
        }
    }
}
