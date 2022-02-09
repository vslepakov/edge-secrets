namespace EdgeSecrets.Samples.SecretManager.Edge
{
    using System;
    using EdgeSecrets.CryptoProvider;

    internal class Configuration
    {
        public static string KeyId => Environment.GetEnvironmentVariable("EDGESECRET_KEYID");

        public static ICryptoProvider CryptoProvider
        {
            get
            {
                var cryptoProviderName = Environment.GetEnvironmentVariable("EDGESECRET_CRYPTO_PROVIDER");
                return cryptoProviderName switch
                {
                    "AzureKeyVault" => new AzureKeyVaultCryptoProvider(),
                    "WorkloadApi" => GetWorkloadApiCryptoProvider(),
                    _ => throw new ArgumentException($"'{cryptoProviderName}' is not a supported crypto provider")
                };
            }
        }

        public static string InfluxDbUrl => Environment.GetEnvironmentVariable("INFLUXDB_URL");

        public static string InfluxDbOrg => Environment.GetEnvironmentVariable("INFLUXDB_ORG");

        public static string InfluxDbBucket => Environment.GetEnvironmentVariable("INFLUXDB_BUCKET");

        private static ICryptoProvider GetWorkloadApiCryptoProvider()
        {
            var initializationVector = Environment.GetEnvironmentVariable("EDGESECRET_INIT_VECTOR");

            if (initializationVector != null)
            {
                Console.WriteLine($"Using initialization vector {initializationVector}");
            }

            return new WorkloadApiCryptoProvider(initializationVector);
        }
    }
}
