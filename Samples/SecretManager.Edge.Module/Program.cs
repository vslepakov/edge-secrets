namespace EdgeSecrets.Samples.SecretManager.Edge.Module
{
    using System;
    using System.Runtime.Loader;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using EdgeSecrets.CryptoProvider;
    using EdgeSecrets.SecretManager;
    using EdgeSecrets.SecretManager.Edge;

    class Program
    {
        static void Main(string[] args)
        {
            Init().Wait();

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

            // Get secrets
            await GetSecrets();
        }

        static async Task GetSecrets()
        {
            (ICryptoProvider? cryptoProvider, KeyOptions? keyOptions) = Configuration.GetCryptoProvider();
            Console.WriteLine($"Using Crypto Provider {cryptoProvider.GetType()}");

            //// Get from file

            // ISecretStore fileSecretStore = new FileSecretStore("/usr/local/cache/secrets.json");
            // ISecretStore secretStore = new InMemorySecretStore(fileSecretStore);
            // var manager = new SecretManagerClient(cryptoProvider, kms, secretStore);

            //// Get from remote

            ISecretStore remoteSecretStore = new RemoteSecretStore(TransportType.Amqp_Tcp_Only);
            ISecretStore fileSecretStore = new FileSecretStore("/usr/local/cache/secrets.json", remoteSecretStore, cryptoProvider, keyOptions);
            ISecretStore secretStore = new InMemorySecretStore(fileSecretStore);
            var manager = new SecretManagerClient(secretStore);
            
            //// Test the secret store

            string keyA = "test";
            var valueA1 = await manager.GetSecretValueAsync(keyA, null, DateTime.Now);
            if (valueA1 != null) Console.WriteLine($"Key '{keyA}' has value '{valueA1}' (first read)");
            var valueA2 = await manager.GetSecretValueAsync(keyA, null, DateTime.Now);
            if (valueA2 != null) Console.WriteLine($"Key '{keyA}' has value '{valueA2}' (second read)");
        }
    }
}
