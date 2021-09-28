﻿using System;
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

            string keyA = "test";

            await manager.SetSecretAsync(keyA, "1234");
            string valueA1 = await manager.GetSecretAsync(keyA);
            Console.WriteLine($"Key {keyA} has value {valueA1}");

            string valueA2 = await manager.GetSecretAsync(keyA);
            Console.WriteLine($"Key {keyA} has value {valueA2}");

            await manager.SetSecretAsync(keyA, "abcdef");
            string valueA3 = await manager.GetSecretAsync(keyA);
            Console.WriteLine($"Key {keyA} has value {valueA3}");

            string keyB = "secret";

            await manager.SetSecretAsync(keyB, "azure");
            string valueB1 = await manager.GetSecretAsync(keyB);
            Console.WriteLine($"Key {keyB} has value {valueB1}");

            string valueB2 = await manager.GetSecretAsync(keyB);
            Console.WriteLine($"Key {keyB} has value {valueB2}");

            await manager.SetSecretAsync(keyB, "veryverysecret");
            string valueB3 = await manager.GetSecretAsync(keyB);
            Console.WriteLine($"Key {keyB} has value {valueB3}");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            GetSecretAsync().GetAwaiter().GetResult();
        }
    }
}
