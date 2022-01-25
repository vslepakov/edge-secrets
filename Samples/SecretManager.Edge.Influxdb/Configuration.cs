namespace EdgeSecrets.Samples.SecretManager.Edge
{
    using System;
    using EdgeSecrets.CryptoProvider;

    internal class Configuration
    {
        public static (ICryptoProvider?, string?) GetCryptoProvider()
        {
            string? keyId = Environment.GetEnvironmentVariable("EDGESECRET_KEYID");
            string? cryptoProviderName = Environment.GetEnvironmentVariable("EDGESECRET_CRYPTO_PROVIDER") ?? "none";

            ICryptoProvider? cryptoProvider;
            switch (cryptoProviderName)
            {
                case "none":
                    cryptoProvider = default;
                    keyId = default;
                    break;
                case "identity-service":
                    cryptoProvider = new IdentityServiceCryptoProvider();
                    break;
                case "workload-api":
                    string? initializationVector = Environment.GetEnvironmentVariable("EDGESECRET_INIT_VECTOR");
                    if (initializationVector != null)
                    {
                        Console.WriteLine($"Using initialization vector {initializationVector}");
                    }
                    cryptoProvider = new WorkloadApiCryptoProvider(initializationVector);
                    break;
                case "azure-kv":
                    cryptoProvider = new AzureKeyVaultCryptoProvider();
                    break;
                default:
                    throw new ArgumentException($"'{cryptoProviderName}' is not a supported crypto provider");
            };

            return (cryptoProvider, keyId);
        }
    }
}
