namespace Tests
{
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;
    using EdgeSecrets.SecretManager;
    using Xunit;

    public class SecretManagerTests
    {
        [Fact]
        public async Task Set_And_Get_Secrets_Using_Test_Provider_Success()
        {
            // Arrange
            const string PLAINTEXT = "Hello World";

            var cryptoProvider = new TestCryptoProvider();

            ISecretStore fileSecretStore = new FileSecretStore("secrets.json");
            ISecretStore secretStore = new InMemorySecretStore(cryptoProvider, new KeyOptions(), fileSecretStore);
            var manager = new SecretManagerClient(cryptoProvider, new KeyOptions(), secretStore);

            // Act
            string key = "testKey";
            await manager.SetSecretValueAsync(key, PLAINTEXT);
            string value = await manager.GetSecretValueAsync(key);

            // Assert
            Assert.False(string.IsNullOrEmpty(value));
            Assert.Equal(PLAINTEXT, value);
        }

        [Fact]
        public async Task Encrypt_And_Decrypt_Data_Using_AzureKeyVault_Provider_Success()
        {
            // Arrange
            const string PLAINTEXT = "Hello World";

            // Create a key in AKV, give your Service Principal access and configure the key reference here 
            //const string KEY_ID = "https://keyvault-ca-2.vault.azure.net/keys/kms-key/84e7576868ff452b918ae5eeb05cf2e0";
            const string KEY_ID = "https://mvsdev3kv.vault.azure.net/keys/kms-key/2da6c35f4fe34496a5078b9a9983f042";

            var cryptoProvider = new AzureKeyVaultCryptoProvider();
            var kms = new KeyOptions 
            {
                KeyId = KEY_ID, 
                KeyType = KeyType.RSA,
                KeySize = 2048
            };

            var fileSecretStore = new FileSecretStore("secrets.json");
            var secretStore = new InMemorySecretStore(cryptoProvider, new KeyOptions(), fileSecretStore);
            var manager = new SecretManagerClient(cryptoProvider, kms, secretStore);

            // Act
            var key = "testKey";
            await manager.SetSecretValueAsync(key, PLAINTEXT);
            var value = await manager.GetSecretValueAsync(key);

            // Assert
            Assert.False(string.IsNullOrEmpty(value));
            Assert.Equal(PLAINTEXT, value);
        }

        [Fact]
        public async Task Encrypt_And_Decrypt_Data_Using_Identity_Service_Provider_With_Asymmetric_Key_Success()
        {
            // Arrange
            const string PLAINTEXT = "Hello World";

            // Create the key first e.g., like this:
            // curl -X POST -H 'Content-Type: application/json' -d '{"keyId": "mytestkey", "preferredAlgorithms": "rsa-2048"}' \
            // --unix-socket /run/aziot/keyd.sock http://keyd.sock/keypair?api-version=2020-09-01
            const string KEY_ID = "mytestkey";

            var cryptoProvider = new IdentityServiceCryptoProvider();
            var kms = new KeyOptions
            {
                KeyId = KEY_ID,
                KeyType = KeyType.RSA,
                KeySize = 2048
            };

            var fileSecretStore = new FileSecretStore("secrets.json");
            var secretStore = new InMemorySecretStore(cryptoProvider, new KeyOptions(), fileSecretStore);
            var manager = new SecretManagerClient(cryptoProvider, kms, secretStore);

            // Act
            var key = "testKey";
            await manager.SetSecretValueAsync(key, PLAINTEXT);
            var value = await manager.GetSecretValueAsync(key);

            // Assert
            Assert.False(string.IsNullOrEmpty(value));
            Assert.Equal(PLAINTEXT, value);
        }

        [Fact]
        public async Task Encrypt_And_Decrypt_Data_Using_Identity_Service_Provider_With_Symmetric_Key_Success()
        {
            // Arrange
            const string PLAINTEXT = "Hello World";

            // Create the key first e.g., like this:
            // curl -X POST -H 'Content-Type: application/json' -d '{"keyId": "mysymmtestkey", "usage": "encrypt"}' \
            // --unix-socket /run/aziot/keyd.sock http://keyd.sock/keypair?api-version=2020-09-01
            const string KEY_ID = "mysymmtestkey";

            var cryptoProvider = new IdentityServiceCryptoProvider();
            var kms = new KeyOptions
            {
                KeyId = KEY_ID,
                KeyType = KeyType.Symmetric
            };

            var fileSecretStore = new FileSecretStore("secrets.json");
            var secretStore = new InMemorySecretStore(cryptoProvider, new KeyOptions(), fileSecretStore);
            var manager = new SecretManagerClient(cryptoProvider, kms, secretStore);

            // Act
            var key = "testKey";
            await manager.SetSecretValueAsync(key, PLAINTEXT);
            var value = await manager.GetSecretValueAsync(key);

            // Assert
            Assert.False(string.IsNullOrEmpty(value));
            Assert.Equal(PLAINTEXT, value);
        }
    }
}
