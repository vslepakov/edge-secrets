namespace EdgeSecrets.Samples.SecretManager.Edge.Module
{
    using System;
    using EdgeSecrets.CryptoProvider;

    internal class Configuration
    {
        public static (ICryptoProvider, KeyOptions) GetCryptoProvider()
        {
            string keyId = Environment.GetEnvironmentVariable("EDGESECRET_KEYID");
            string cryptoProviderName = Environment.GetEnvironmentVariable("EDGESECRET_CRYPTO_PROVIDER") ?? "none";

            KeyOptions keyOptions;
            ICryptoProvider cryptoProvider;
            switch (cryptoProviderName)
            {
                case "none":
                    cryptoProvider = null;
                    keyOptions = null;
                    break;
                case "identity-service":
                    cryptoProvider = new IdentityServiceCryptoProvider();
                    keyOptions = new KeyOptions
                    {
                        KeyId = keyId,
                        KeyType = KeyType.RSA,
                        KeySize = 2048
                    };
                    break;
                case "workload-api":
                    string initializationVector = Environment.GetEnvironmentVariable("EDGESECRET_INIT_VECTOR");
                    cryptoProvider = new WorkloadApiCryptoProvider(initializationVector);
                    keyOptions = new KeyOptions
                    {
                        KeyId = keyId,
                        KeyType = KeyType.Symmetric,
                        KeySize = 2048
                    };
                    break;
                case "azure-kv":
                    cryptoProvider = new AzureKeyVaultCryptoProvider();
                    keyOptions = new KeyOptions
                    {
                        KeyId = keyId,
                        KeyType = KeyType.RSA,
                        KeySize = 2048
                    };
                    break;
                default:
                    throw new ArgumentException($"'{cryptoProviderName}' is not a supported crypto provider");
            };

            return (cryptoProvider, keyOptions);
        }
    }
}
