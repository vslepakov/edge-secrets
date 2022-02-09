namespace EdgeSecrets.Samples.SecretManager.Edge
{
    using System;
    using System.Runtime.Loader;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using EdgeSecrets.SecretManager;
    using InfluxDB.Client;

    class Program
    {
        private const string SecretsFile = "/usr/local/cache/secrets.json";
        private static ModuleClient _moduleClient;

        static void Main(string[] args)
        {
            Init().Wait();

            ReadDatabase().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            TaskCompletionSource<bool>? taskCompletionSource = new();
            cancellationToken.Register(s =>
            {
                var tcs = (TaskCompletionSource<bool>?)s;
                if (tcs is not null)
                {
                    tcs.SetResult(true);
                }
            }, taskCompletionSource);
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            _moduleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await _moduleClient.OpenAsync();

            Console.WriteLine("IoT Hub module client initialized.");
        }

        static async Task ReadDatabase()
        {
            Console.WriteLine("Get data from database.");

            var cryptoProvider = Configuration.CryptoProvider;
            Console.WriteLine($"Using Crypto Provider {cryptoProvider?.GetType()}");

            var secretManagerClient = new SecretManagerClient()
                .WithRemoteSecretStore(_moduleClient)
                .WithFileSecretStore(SecretsFile, cryptoProvider, Configuration.KeyId)
                .WithInMemorySecretStore();

            Console.WriteLine("Secret manager client created.");

            var dbUsername = await secretManagerClient.GetSecretValueAsync("InfluxDbUsername", null, DateTime.Now);
            var dbPassword = await secretManagerClient.GetSecretValueAsync("InfluxDbPassword", null, DateTime.Now);

            if (string.IsNullOrEmpty(dbUsername))
            {
                Console.WriteLine($"ERROR, no username found!");
                return;
            }

            if (string.IsNullOrEmpty(dbPassword))
            {
                Console.WriteLine($"ERROR, no password found!");
                return;
            }

            Console.WriteLine($"Valid secret found.");

            InfluxDBClient influxDBClient = null;

            try
            {
                influxDBClient = InfluxDBClientFactory.Create(Configuration.InfluxDbUrl, dbUsername, dbPassword.ToCharArray());
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
    }
}
