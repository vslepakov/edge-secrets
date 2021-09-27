using System;
using System.Threading.Tasks;
using EdgeSecrets.KeyManagement;
using EdgeSecrets.Samples.SecretManager.Common;

namespace EdgeSecrets.Samples.SecretManager.Host
{
    class Program
    {
        static async Task GetSecretAsync()
        {
            ICryptoProvider cryptoProvider = new MyTestCryptoProvider();
            ISecretStore fileSecretStore = new FileSecretStore("secrets.json");
            ISecretStore secretStore = new InMemoryCacheSecretStore(fileSecretStore);
            var manager = new SecretManager.Common.SecretManager(cryptoProvider, secretStore);

            string key = "test";

            await manager.SetSecretAsync(key, "1234");
            string value1 = await manager.GetSecretAsync(key);
            Console.WriteLine($"Key {key} has value {value1}");

            string value2 = await manager.GetSecretAsync(key);
            Console.WriteLine($"Key {key} has value {value2}");

            await manager.SetSecretAsync(key, "abcdef");
            string value3 = await manager.GetSecretAsync(key);
            Console.WriteLine($"Key {key} has value {value3}");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            GetSecretAsync().GetAwaiter().GetResult();
        }
    }
}
