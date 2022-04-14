namespace Tests.SecretManagerTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;
    using EdgeSecrets.SecretManager;
    using Xunit;

    public class SecretManagerTests
    {
        const string FILENAME = "secrets.json";
        const string PLAINTEXT = "Hello World";

        // Create a key in AKV, give your Service Principal access and configure the key reference here 
        const string AKV_KEY_ID = "https://mvsdev3kv.vault.azure.net/keys/kms-key/2da6c35f4fe34496a5078b9a9983f042";

        // Create a key like that:
        // curl -X POST -H 'Content-Type: application/json' -d '{"keyId": "mysymmtestkey1", "usage": "encrypt"}'  --unix-socket /run/aziot/keyd.sock http://keyd.sock/key?api-version=2020-09-01
        const string IS_KEY_ID = "mysymmtestkey1";

        [Fact]
        public async Task Encrypt_And_Decrypt_Data_Using_AzureKeyVault_Provider_Success()
        {
            // Arrange
            var cryptoProvider = new AzureKeyVaultCryptoProvider();

            // Act
            var encryptedValue = await cryptoProvider.EncryptAsync(PLAINTEXT, AKV_KEY_ID);
            var decryptedValue = await cryptoProvider.DecryptAsync(encryptedValue, AKV_KEY_ID);

            // Assert
            Assert.False(string.IsNullOrEmpty(encryptedValue));
            Assert.False(string.IsNullOrEmpty(decryptedValue));
            Assert.Equal(PLAINTEXT, decryptedValue);
        }

        [Fact]
        public async Task Encrypt_And_Decrypt_Data_Using_IdentityService_Provider_Success()
        {
            // Arrange
            var cryptoProvider = new IdentityServiceCryptoProvider();

            // Act
            var encryptedValue = await cryptoProvider.EncryptAsync(PLAINTEXT, IS_KEY_ID);
            var decryptedValue = await cryptoProvider.DecryptAsync(encryptedValue, IS_KEY_ID);

            // Assert
            Assert.False(string.IsNullOrEmpty(encryptedValue));
            Assert.False(string.IsNullOrEmpty(decryptedValue));
            Assert.Equal(PLAINTEXT, decryptedValue);
        }

        [Fact]
        public async Task Set_And_Get_Secret_Using_InMemory_SecretStore_And_Test_Provider_Success()
        {
            // Arrange
            var cryptoProvider = new TestCryptoProvider();

            var manager = new SecretManagerClient()
                .WithInMemorySecretStore(cryptoProvider);

            // Act
            string secretName = "test";
            string secretVersion = "v1";
            await manager.SetSecretValueAsync(secretName, secretVersion, PLAINTEXT);
            var secretValue = await manager.GetSecretValueAsync(secretName, null, DateTime.Now);

            // Assert
            Assert.False(string.IsNullOrEmpty(secretValue));
            Assert.Equal(PLAINTEXT, secretValue);
        }

        [Fact]
        public async Task Set_And_Get_Versioned_Secret_Using_InMemory_SecretStore_And_Test_Provider_Success()
        {
            // Arrange
            var cryptoProvider = new TestCryptoProvider();

            var manager = new SecretManagerClient()
                .WithInMemorySecretStore(cryptoProvider);

            string secretName = "test";

            // Act
            var secrets = await manager.GetSecretListAsync(new List<string>() { secretName});

            string version1Name = "v1";
            await manager.SetSecretValueAsync(secretName, version1Name, PLAINTEXT);
            secrets = await manager.GetSecretListAsync(new List<string>() { secretName});
            var version1SecretValue = await manager.GetSecretValueAsync(secretName, version1Name, DateTime.Now);
            var anySecretValue = await manager.GetSecretValueAsync(secretName, null, DateTime.Now);

            // Assert
            Assert.False(string.IsNullOrEmpty(version1SecretValue));
            Assert.Equal(PLAINTEXT, version1SecretValue);
            Assert.False(string.IsNullOrEmpty(anySecretValue));

            // Act
            string version2Name = "v2";
            await manager.SetSecretValueAsync(secretName, version2Name, PLAINTEXT + PLAINTEXT);
            secrets = await manager.GetSecretListAsync(new List<string>() { secretName});
            version1SecretValue = await manager.GetSecretValueAsync(secretName, version1Name, DateTime.Now);
            var version2SecretValue = await manager.GetSecretValueAsync(secretName, version2Name, DateTime.Now);
            anySecretValue = await manager.GetSecretValueAsync(secretName, null, DateTime.Now);

            // Assert
            Assert.False(string.IsNullOrEmpty(version1SecretValue));
            Assert.Equal(PLAINTEXT, version1SecretValue);
            Assert.False(string.IsNullOrEmpty(version2SecretValue));
            Assert.Equal(PLAINTEXT + PLAINTEXT, version2SecretValue);
            Assert.False(string.IsNullOrEmpty(anySecretValue));
        }

        [Fact]
        public async Task Set_And_Get_Secret_Using_File_SecretStore_And_Test_Provider_Success()
        {
            var cryptoProvider = new TestCryptoProvider();

            var manager = new SecretManagerClient()
                .WithFileSecretStore(FILENAME, cryptoProvider);

            // Act
            string secretName = "test";
            string secretVersion = "v1";
            await manager.SetSecretValueAsync(secretName, secretVersion, PLAINTEXT);
            var secretValue = await manager.GetSecretValueAsync(secretName, null, DateTime.Now);

            // Assert
            Assert.False(string.IsNullOrEmpty(secretValue));
            Assert.Equal(PLAINTEXT, secretValue);
        }

        [Fact]
        public async Task Set_And_Get_Secret_Using_File_And_InMemory_SecretStore_And_Test_Provider_Success()
        {
            var cryptoProvider = new TestCryptoProvider();

            var manager = new SecretManagerClient()
                .WithFileSecretStore(FILENAME, cryptoProvider)
                .WithInMemorySecretStore();

            // Act
            string secretName = "test";
            string secretVersion = "v1";
            await manager.SetSecretValueAsync(secretName, secretVersion, PLAINTEXT);
            var secretValue = await manager.GetSecretValueAsync(secretName, null, DateTime.Now);

            // Assert
            Assert.False(string.IsNullOrEmpty(secretValue));
            Assert.Equal(PLAINTEXT, secretValue);
        }

        [Fact]
        public async Task Set_And_Get_Versioned_Secret_Using_File_And_InMemory_SecretStore_And_Test_Provider_Success()
        {
            // Arrange
            var cryptoProvider = new TestCryptoProvider();

            var manager = new SecretManagerClient()
                .WithFileSecretStore(FILENAME, cryptoProvider)
                .WithInMemorySecretStore();

            string secretName = "test";

            // Act
            var secrets = await manager.GetSecretListAsync(new List<string>() { secretName });

            string version1Name = "v1";
            await manager.SetSecretValueAsync(secretName, version1Name, PLAINTEXT);
            secrets = await manager.GetSecretListAsync(new List<string>() { secretName });
            var version1SecretValue = await manager.GetSecretValueAsync(secretName, version1Name, DateTime.Now);
            var anySecretValue = await manager.GetSecretValueAsync(secretName, null, DateTime.Now);

            // Assert
            Assert.False(string.IsNullOrEmpty(version1SecretValue));
            Assert.Equal(PLAINTEXT, version1SecretValue);
            Assert.False(string.IsNullOrEmpty(anySecretValue));

            // Act
            string version2Name = "v2";
            await manager.SetSecretValueAsync(secretName, version2Name, PLAINTEXT + PLAINTEXT);
            secrets = await manager.GetSecretListAsync(new List<string>() { secretName });
            version1SecretValue = await manager.GetSecretValueAsync(secretName, version1Name, DateTime.Now);
            var version2SecretValue = await manager.GetSecretValueAsync(secretName, version2Name, DateTime.Now);
            anySecretValue = await manager.GetSecretValueAsync(secretName, null, DateTime.Now);

            // Assert
            Assert.False(string.IsNullOrEmpty(version1SecretValue));
            Assert.Equal(PLAINTEXT, version1SecretValue);
            Assert.False(string.IsNullOrEmpty(version2SecretValue));
            Assert.Equal(PLAINTEXT + PLAINTEXT, version2SecretValue);
            Assert.False(string.IsNullOrEmpty(anySecretValue));
        }

        [Fact]
        public async Task Set_And_Get_Secret_Using_File_And_InMemory_SecretStore_And_AzureKeyVault_Provider_Success()
        {
            // Arrange
            var cryptoProvider = new AzureKeyVaultCryptoProvider();

            var manager = new SecretManagerClient()
                .WithFileSecretStore(FILENAME, cryptoProvider, AKV_KEY_ID)
                .WithInMemorySecretStore();
            await manager.ClearCacheAsync();

            // Act
            string secretName = "test";
            string secretVersion = "v1";
            await manager.SetSecretValueAsync(secretName, secretVersion, PLAINTEXT);
            var secretValue = await manager.GetSecretValueAsync(secretName, null, DateTime.Now);

            // Assert
            Assert.False(string.IsNullOrEmpty(secretValue));
            Assert.Equal(PLAINTEXT, secretValue);
        }

        [Fact]
        public async Task Set_And_Get_Secret_Using_File_And_InMemory_Secret_Store_And_IdentityService_Provider_Success()
        {
            // Arrange
            var cryptoProvider = new IdentityServiceCryptoProvider();

            var manager = new SecretManagerClient()
                .WithFileSecretStore(FILENAME, cryptoProvider, IS_KEY_ID)
                .WithInMemorySecretStore();
            await manager.ClearCacheAsync();


            // Act
            string secretName = "test";
            string secretVersion = "v1";
            await manager.SetSecretValueAsync(secretName, secretVersion, PLAINTEXT);
            var secretValue = await manager.GetSecretValueAsync(secretName, null, DateTime.Now);

            // Assert
            Assert.False(string.IsNullOrEmpty(secretValue));
            Assert.Equal(PLAINTEXT, secretValue);
        }

        [Fact]
        public async Task Set_And_Get_Multiple_Secrets_Using_File_And_InMemory_SecretStore_And_AzureKeyVault_Provider_Success()
        {
            // Arrange
            var cryptoProvider = new AzureKeyVaultCryptoProvider();

            string keyA = "keyA", valueA = $"Value of {keyA}";
            string keyB = "keyB", valueB = $"Value of {keyB}";
            string keyC = "keyC", valueC = $"Value of {keyC}";
            string keyD = "keyD", valueD = $"Value of {keyD}";

            // 1. Clear secret stores and store new secrets then get the secret values
            // Act
            var manager = new SecretManagerClient()
                .WithFileSecretStore(FILENAME, cryptoProvider, AKV_KEY_ID)
                .WithInMemorySecretStore();
            await manager.ClearCacheAsync();

            string secretVersion = "v1";
            await manager.SetSecretValueAsync(keyA, secretVersion, valueA);
            await manager.SetSecretValueAsync(keyB, secretVersion, valueB);
            await manager.SetSecretValueAsync(keyC, secretVersion, valueC);
            await manager.SetSecretValueAsync(keyD, secretVersion, valueD);

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
                .WithFileSecretStore(FILENAME, cryptoProvider, AKV_KEY_ID)
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
            var fileSecretStore = new FileSecretStore(FILENAME, null, cryptoProvider, AKV_KEY_ID);
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