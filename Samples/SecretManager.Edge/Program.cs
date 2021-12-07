namespace EdgeSecrets.Samples.SecretManager.Edge
{
    using System;
    using System.Runtime.Loader;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Data.SqlClient;
    using EdgeSecrets.CryptoProvider;
    using EdgeSecrets.SecretManager;

    class Program
    {
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
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");
        }

        static async Task ReadDatabase()
        {
            Console.WriteLine("Get data from database.");

            (ICryptoProvider? cryptoProvider, string? keyId) = Configuration.GetCryptoProvider();
            Console.WriteLine($"Using Crypto Provider {cryptoProvider?.GetType()}");

            string secretsFile = "/usr/local/cache/secrets.json";
            var secretManagerClient = new SecretManagerClient()
                .WithRemoteSecretStore(TransportType.Mqtt_Tcp_Only, new ClientOptions())
                .WithFileSecretStore(secretsFile, cryptoProvider, keyId)
                .WithInMemorySecretStore();
            Console.WriteLine("Secret manager client created.");

            string? dbConnectionString = await secretManagerClient.GetSecretValueAsync("FabrikamConnectionString", null, DateTime.Now);
            if (!string.IsNullOrEmpty(dbConnectionString))
            {
                Console.WriteLine($"Valid database connection string found.");

                using SqlConnection connection = new SqlConnection(dbConnectionString);
                String sql = "SELECT ProductID, Name, ProductNumber, Color, Size, Weight FROM [SalesLT].[Product]";

                using SqlCommand command = new SqlCommand(sql, connection);
                connection.Open();

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"{reader.GetInt32(0)} {reader.GetString(1)} {reader.GetString(2)}");
                }
            }
        }
    }
}
