namespace Tests
{
    using System;
    using System.Collections.Generic;
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

            var manager = new SecretManagerClient()
                .WithFileSecretStore("secrets.json", cryptoProvider)
                .WithInMemorySecretStore();

            // Act
            string key = "testKey";
            await manager.SetSecretValueAsync(key, PLAINTEXT);
            var value = await manager.GetSecretValueAsync(key, null, DateTime.Now);

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

            var manager = new SecretManagerClient()
                .WithFileSecretStore("secrets.json", cryptoProvider, KEY_ID)
                .WithInMemorySecretStore();
            await manager.ClearCacheAsync();

            // Act
            var key = "testKey";
            await manager.SetSecretValueAsync(key, PLAINTEXT);
            var value = await manager.GetSecretValueAsync(key, null, DateTime.Now);

            // Assert
            Assert.False(string.IsNullOrEmpty(value));
            Assert.Equal(PLAINTEXT, value);
        }

        [Fact]
        public async Task Encrypt_And_Decrypt_Data_Using_Identity_Service_Provider_Success()
        {
            // Arrange
            const string PLAINTEXT = "Hello World";

            // Create the key first e.g., like this:
            // curl -X POST -H 'Content-Type: application/json' -d '{"keyId": "mytestkey", "preferredAlgorithms": "rsa-2048"}' \
            // --unix-socket /run/aziot/keyd.sock http://keyd.sock/keypair?api-version=2020-09-01
            const string KEY_ID = "mysymmtestkey";

            var cryptoProvider = new IdentityServiceCryptoProvider();

            var manager = new SecretManagerClient()
                .WithFileSecretStore("secrets.json", cryptoProvider, KEY_ID)
                .WithInMemorySecretStore();
            await manager.ClearCacheAsync();


            // Act
            var key = "testKey";
            await manager.SetSecretValueAsync(key, PLAINTEXT);
            var value = await manager.GetSecretValueAsync(key, null, DateTime.Now);

            // Assert
            Assert.False(string.IsNullOrEmpty(value));
            Assert.Equal(PLAINTEXT, value);
        }

        [Fact]
        public async Task Set_And_Get_Secrets_Using_Secret_Store_Chain_Success()
        {
            // Arrange
            var cryptoProvider = new TestCryptoProvider();

            string keyA = "keyA", valueA = $"Value of {keyA}";
            string keyB = "keyB", valueB = $"Value of {keyB}";
            string keyC = "keyC", valueC = $"Value of {keyC}";
            string keyD = "keyD", valueD = $"Value of {keyD}";

            // 1. Clear secret stores and store new secrets then get the secret values
            // Act
            var manager = new SecretManagerClient()
                .WithFileSecretStore("secrets.json", cryptoProvider)
                .WithInMemorySecretStore();
            await manager.ClearCacheAsync();

            await manager.SetSecretValueAsync(keyA, valueA);
            await manager.SetSecretValueAsync(keyB, valueB);
            await manager.SetSecretValueAsync(keyC, valueC);
            await manager.SetSecretValueAsync(keyD, valueD);

            var ret1A = await manager.GetSecretValueAsync(keyA, null, DateTime.Now);
            var ret1B = await manager.GetSecretValueAsync(keyB, null, DateTime.Now);
            var ret1C = await manager.GetSecretValueAsync(keyC, null, DateTime.Now);
            var ret1D = await manager.GetSecretValueAsync(keyD, null, DateTime.Now);

            // Assert
            Assert.Equal(valueA, ret1A);
            Assert.Equal(valueB, ret1B);
            Assert.Equal(valueC, ret1C);
            Assert.Equal(valueD, ret1D);

            // 2. Get the secret values from previously stored secrets
            // Act
            manager = new SecretManagerClient()
                .WithFileSecretStore("secrets.json", cryptoProvider)
                .WithInMemorySecretStore();

            // Assert
            var ret2A = await manager.GetSecretValueAsync(keyA, null, DateTime.Now);
            var ret2B = await manager.GetSecretValueAsync(keyB, null, DateTime.Now);
            var ret2C = await manager.GetSecretValueAsync(keyC, null, DateTime.Now);
            var ret2D = await manager.GetSecretValueAsync(keyD, null, DateTime.Now);

            // Assert
            Assert.Equal(valueA, ret2A);
            Assert.Equal(valueB, ret2B);
            Assert.Equal(valueC, ret2C);
            Assert.Equal(valueD, ret2D);                

            // 3. Get a secret list from previously stored secrets
            // Act
            var fileSecretStore = new FileSecretStore("secrets.json", null, cryptoProvider);
            var inMemorySecretStore = new InMemorySecretStore(fileSecretStore);
            manager = new SecretManagerClient(inMemorySecretStore);

            // Assert
            Assert.True(inMemorySecretStore.LocalSecretCount == 0); // no secrets cached yet
            Assert.True(fileSecretStore.LocalSecretCount == 4); // all secrets are stored in persistant file

            var ret3a = await manager.GetSecretListAsync(new List<string>() { keyA, keyB });
            Assert.False(ret3a == null);
            Assert.True(ret3a.Values.Count == 2);
            Assert.False(ret3a.GetSecret(keyA, null) == null);
            Assert.Equal(valueA, ret3a.GetSecret(keyA, null).Value);
            Assert.False(ret3a.GetSecret(keyB, null) == null);
            Assert.Equal(valueB, ret3a.GetSecret(keyB, null).Value);
            Assert.True(inMemorySecretStore.LocalSecretCount == 2);
            Assert.True(fileSecretStore.LocalSecretCount == 4);

            var ret3b = await manager.GetSecretListAsync(new List<string>() { keyC, keyD });
            Assert.False(ret3b == null);
            Assert.True(ret3b.Values.Count == 2);
            Assert.False(ret3b.GetSecret(keyC, null) == null);
            Assert.Equal(valueC, ret3b.GetSecret(keyC, null).Value);
            Assert.False(ret3b.GetSecret(keyD, null) == null);
            Assert.Equal(valueD, ret3b.GetSecret(keyD, null).Value);
            Assert.True(inMemorySecretStore.LocalSecretCount == 4);
            Assert.True(fileSecretStore.LocalSecretCount == 4);

            var ret3c = await manager.GetSecretListAsync(new List<string>() { keyA, keyB, keyC, keyD });
            Assert.False(ret3c == null);
            Assert.True(ret3c.Values.Count == 4);
            Assert.True(inMemorySecretStore.LocalSecretCount == 4);
            Assert.True(fileSecretStore.LocalSecretCount == 4);
        }
    }
}