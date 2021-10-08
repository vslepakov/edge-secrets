﻿using System.Threading.Tasks;
using EdgeSecrets.KeyManagement;
using EdgeSecrets.Samples.SecretManager.Common;
using Xunit;

namespace Tests
{
    public class SecretManagerTests
    {
        [Fact]
        public async Task Set_And_Get_Secrets_Using_Test_Provider_Success()
        {
            // Arrange
            const string PLAINTEXT = "Hello World";

            var cryptoProvider = new TestCryptoProvider();

            ISecretStore fileSecretStore = new FileSecretStore("secrets.json");
            ISecretStore secretStore = new InMemoryCacheSecretStore(fileSecretStore);
            var manager = new EdgeSecrets.Samples.SecretManager.Common.SecretManager(cryptoProvider, new KeyOptions(), secretStore);

            // Act
            string key = "testKey";
            await manager.SetSecretAsync(key, PLAINTEXT);
            string value = await manager.GetSecretAsync(key);

            // Assert
            Assert.False(string.IsNullOrEmpty(value));
            Assert.Equal(PLAINTEXT, value);
        }

        [Fact]
        public async Task Encrypt_And_Decrypt_Data_Using_AzureKeyVault_Provider_Success()
        {
            // Arrange
            const string PLAINTEXT = "Hello World";
            //const string KEY_ID = "https://keyvault-ca-2.vault.azure.net/keys/kms-key/84e7576868ff452b918ae5eeb05cf2e0";
            const string KEY_ID = "https://mvsdev3kv.vault.azure.net/keys/kms-key/2da6c35f4fe34496a5078b9a9983f042";

            var cryptoProvider = new AzureKeyVaultCryptoProvider();
            var kms = new KeyOptions 
            {
                KeyId = KEY_ID, 
                KeyType = KeyType.RSA,
                KeySize = 2048
            };

            ISecretStore fileSecretStore = new FileSecretStore("secrets.json");
            ISecretStore secretStore = new InMemoryCacheSecretStore(fileSecretStore);
            var manager = new EdgeSecrets.Samples.SecretManager.Common.SecretManager(cryptoProvider, kms, secretStore);

            // Act
            string key = "testKey";
            await manager.SetSecretAsync(key, PLAINTEXT);
            string value = await manager.GetSecretAsync(key);

            // Assert
            Assert.False(string.IsNullOrEmpty(value));
            Assert.Equal(PLAINTEXT, value);
        }
    }
}
