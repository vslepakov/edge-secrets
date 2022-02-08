using EdgeSecrets.CryptoProvider;
using System;

namespace SecretManager.Host
{
    internal class Configuration
    {
        public static string KeyId => Environment.GetEnvironmentVariable("EDGESECRET_KEYID");

        public static ICryptoProvider CryptoProvider
        { 
            get
            {
                var provider = Environment.GetEnvironmentVariable("EDGESECRET_CRYPTO_PROVIDER");

                return provider switch
                {
                    "AzureKeyVault" => new AzureKeyVaultCryptoProvider(),
                    "IdentityService" => new IdentityServiceCryptoProvider(),
                    _ => new IdentityServiceCryptoProvider(),
                };
            } 
        }

        public static string InfluxDbUrl => Environment.GetEnvironmentVariable("INFLUXDB_URL");

        public static string InfluxDbOrg => Environment.GetEnvironmentVariable("INFLUXDB_ORG");

        public static string InfluxDbBucket => Environment.GetEnvironmentVariable("INFLUXDB_BUCKET");
    }
}
