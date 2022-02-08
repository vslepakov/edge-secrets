namespace EdgeSecrets.Samples.SecretManager.Host
{
    using System;
    using System.Threading.Tasks;
    using EdgeSecrets.SecretManager;
    using global::SecretManager.Host;
    using InfluxDB.Client;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Net.Http.Headers;

    class Program
    {
        private const string SecretsFile = "/usr/local/cache/secrets.json";
        private static readonly long SasTokenLifeTime = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

        static async Task Main(string[] args)
        {
            Console.WriteLine("SecretManager test");
            await ReadDatabaseAsync();
        }

        private static async Task ReadDatabaseAsync()
        {
            Console.WriteLine("Get data from database.");

            var cryptoProvider = Configuration.CryptoProvider;
            var udsHttpClientFactory = new UdsHttpClientFactory();
            var identityServiceClient = new IdentityServiceClient(udsHttpClientFactory);

            var connectionString = await identityServiceClient.GetModuleConnectionStringAsync(SasTokenLifeTime);
            var moduleClient = ModuleClient.CreateFromConnectionString(connectionString);

            Console.WriteLine($"Using Crypto Provider {cryptoProvider?.GetType()}");

            var secretManagerClient = new SecretManagerClient()
                .WithRemoteSecretStore(moduleClient)
                .WithFileSecretStore(SecretsFile, cryptoProvider, Configuration.KeyId)
                .WithInMemorySecretStore();

            Console.WriteLine("Secret manager client created.");

            var dbPassword = await secretManagerClient.GetSecretValueAsync("InfluxDbPassword", null, DateTime.Now);

            if (!string.IsNullOrEmpty(dbPassword))
            {
                Console.WriteLine($"Valid secret found.");

                InfluxDBClient influxDBClient = null;

                try
                {
                    influxDBClient = InfluxDBClientFactory.Create(Configuration.InfluxDbUrl, Configuration.InfluxDbUsername, dbPassword.ToCharArray());
                    var flux = string.Format($"from(bucket:\"{Configuration.InfluxDbBucket}\") |> range(start: 0)");
                    var fluxTables = await influxDBClient.GetQueryApi().QueryAsync(flux, Configuration.InfluxDbOrg);

                    fluxTables.ForEach(fluxTable =>
                    {
                        var fluxRecords = fluxTable.Records;
                        fluxRecords.ForEach(fluxRecord =>
                        {
                            Console.WriteLine($"{fluxRecord.GetTime()}: {fluxRecord.GetValue()}");
                        });
                    });
                }
                finally
                {
                    influxDBClient?.Dispose();
                }
            }
            else
            {
                Console.WriteLine($"ERROR, no secret found!");
            }
        }
    }
}
