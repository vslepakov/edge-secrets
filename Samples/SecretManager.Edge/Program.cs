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

    class Program
    {
        static int counter;

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

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);

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

            ISecretStore fileSecretStore = new FileSecretStore("/usr/local/cache/secrets.json");
            ISecretStore secretStore = new InMemorySecretStore(fileSecretStore);
            var manager = new SecretManagerClient(cryptoProvider, kms, secretStore);
            Console.WriteLine($"EdgeSecret test using Crypto Provider {cryptoProvider}");

            string keyA = "test";

            await manager.SetSecretAsync(keyA, "1234");
            string valueA1 = await manager.GetSecretAsync(keyA);
            Console.WriteLine($"Key '{keyA}' has value '{valueA1}'");

            string valueA2 = await manager.GetSecretAsync(keyA);
            Console.WriteLine($"Key '{keyA}' has value '{valueA2}'");

            await manager.SetSecretAsync(keyA, "abcdef");
            string valueA3 = await manager.GetSecretAsync(keyA);
            Console.WriteLine($"Key '{keyA}' has value '{valueA3}'");

            string keyB = "secret";

            await manager.SetSecretAsync(keyB, "azure");
            string valueB1 = await manager.GetSecretAsync(keyB);
            Console.WriteLine($"Key '{keyB}' has value '{valueB1}'");

            string valueB2 = await manager.GetSecretAsync(keyB);
            Console.WriteLine($"Key '{keyB}' has value '{valueB2}'");

            await manager.SetSecretAsync(keyB, "veryverysecret");
            string valueB3 = await manager.GetSecretAsync(keyB);
            Console.WriteLine($"Key '{keyB}' has value '{valueB3}'");
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            int counterValue = Interlocked.Increment(ref counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

            if (!string.IsNullOrEmpty(messageString))
            {
                using (var pipeMessage = new Message(messageBytes))
                {
                    foreach (var prop in message.Properties)
                    {
                        pipeMessage.Properties.Add(prop.Key, prop.Value);
                    }
                    await moduleClient.SendEventAsync("output1", pipeMessage);
                
                    Console.WriteLine("Received message sent");
                }
            }
            return MessageResponse.Completed;
        }
    }
}
