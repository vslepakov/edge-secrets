namespace EdgeSecrets.Samples.SecretManager.Edge
{
    using System;
    using System.Runtime.Loader;
    using System.Text;
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
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
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
            string KEY_ID = Environment.GetEnvironmentVariable("EDGESECRET_KEYID");
            string CRYPTO_PROVIDER = Environment.GetEnvironmentVariable("EDGESECRET_CRYPTO_PROVIDER");
            //TO DO: pass over the Initialization Vector
            string INIT_VECTOR = Environment.GetEnvironmentVariable("EDGESECRET_INIT_VECTOR");

            ICryptoProvider cryptoProvider;
            KeyOptions kms;
            switch (CRYPTO_PROVIDER)
            {
                default:
                    throw new ArgumentException($"'{CRYPTO_PROVIDER}' is not a supported crypto provider");
                                    
                case "workload-api":
                    cryptoProvider = new WorkloadApiCryptoProvider();

                    //TO DO: pass over the Initialization Vector to WorkloadApiCryptoProvider
					Console.WriteLine($"initialization vector '{INIT_VECTOR}'");
                    
                    kms = new KeyOptions 
                    {
                        KeyId = KEY_ID, 
                        KeyType = KeyType.Symmetric,
                        KeySize = 2048
                    };
                    break;

                case "azure-kv":
                    cryptoProvider = new AzureKeyVaultCryptoProvider();

                    kms = new KeyOptions 
                    {
                        KeyId = KEY_ID, 
                        KeyType = KeyType.RSA,
                        KeySize = 2048
                    };
                    break;
            }; 

            //// Get from file

            // ISecretStore fileSecretStore = new FileSecretStore("/usr/local/cache/secrets.json");
            // ISecretStore secretStore = new InMemorySecretStore(fileSecretStore);
            // var manager = new SecretManagerClient(cryptoProvider, kms, secretStore);
            // Console.WriteLine($"EdgeSecret test using Crypto Provider {cryptoProvider}");

            // string keyA = "test";

            // await manager.SetSecretValueAsync(keyA, "1234");
            // string valueA1 = await manager.GetSecretValueAsync(keyA);
            // Console.WriteLine($"Key '{keyA}' has value '{valueA1}'");

            // string valueA2 = await manager.GetSecretValueAsync(keyA);
            // Console.WriteLine($"Key '{keyA}' has value '{valueA2}'");

            // await manager.SetSecretValueAsync(keyA, "abcdef");
            // string valueA3 = await manager.GetSecretValueAsync(keyA);
            // Console.WriteLine($"Key '{keyA}' has value '{valueA3}'");

            // string keyB = "secret";

            // await manager.SetSecretValueAsync(keyB, "azure");
            // string valueB1 = await manager.GetSecretValueAsync(keyB);
            // Console.WriteLine($"Key '{keyB}' has value '{valueB1}'");

            // string valueB2 = await manager.GetSecretValueAsync(keyB);
            // Console.WriteLine($"Key '{keyB}' has value '{valueB2}'");

            // await manager.SetSecretValueAsync(keyB, "veryverysecret");
            // string valueB3 = await manager.GetSecretValueAsync(keyB);
            // Console.WriteLine($"Key '{keyB}' has value '{valueB3}'");

            //// Get from remote

            ISecretStore remoteSecretStore = new RemoteSecretStore(TransportType.Amqp_Tcp_Only);
            ISecretStore secretStore = new InMemorySecretStore(remoteSecretStore);
            var manager = new SecretManagerClient(secretStore);
            Console.WriteLine($"EdgeSecret test using Crypto Provider {cryptoProvider}");

            string keyA = "test";

            string valueA1 = await manager.GetSecretValueAsync(keyA, DateTime.Now);
            Console.WriteLine($"Key '{keyA}' has value '{valueA1}'");
        }
    }
}
