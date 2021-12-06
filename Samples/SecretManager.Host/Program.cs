namespace EdgeSecrets.Samples.SecretManager.Host
{
    using System;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;
    using EdgeSecrets.SecretManager;

    class Program
    {
        static async Task GetSecretAsync()
        {
            string? KEY_ID = Environment.GetEnvironmentVariable("EDGESECRET_KEYID");

            var cryptoProvider = new AzureKeyVaultCryptoProvider();

            string secretsFile = "/usr/local/cache/secrets.json";

            var manager = new SecretManagerClient()
                .WithFileSecretStore(secretsFile, cryptoProvider)
                .WithInMemorySecretStore();

            Console.WriteLine($"EdgeSecret test using Crypte Provider {cryptoProvider}");

            string keyA = "test";

            await manager.SetSecretValueAsync(keyA, "1234");
            string? valueA1 = await manager.GetSecretValueAsync(keyA, null, DateTime.Now);
            Console.WriteLine($"Key '{keyA}' has value '{valueA1}'");

            string? valueA2 = await manager.GetSecretValueAsync(keyA, null, DateTime.Now);
            Console.WriteLine($"Key '{keyA}' has value '{valueA2}'");

            await manager.SetSecretValueAsync(keyA, "abcdef");
            string? valueA3 = await manager.GetSecretValueAsync(keyA, null, DateTime.Now);
            Console.WriteLine($"Key '{keyA}' has value '{valueA3}'");

            string keyB = "secret";

            await manager.SetSecretValueAsync(keyB, "azure");
            string? valueB1 = await manager.GetSecretValueAsync(keyB, null, DateTime.Now);
            Console.WriteLine($"Key '{keyB}' has value '{valueB1}'");

            string? valueB2 = await manager.GetSecretValueAsync(keyB, null, DateTime.Now);
            Console.WriteLine($"Key '{keyB}' has value '{valueB2}'");

            await manager.SetSecretValueAsync(keyB, "veryverysecret");
            string? valueB3 = await manager.GetSecretValueAsync(keyB, null, DateTime.Now);
            Console.WriteLine($"Key '{keyB}' has value '{valueB3}'");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("SecretManager test");
            GetSecretAsync().GetAwaiter().GetResult();
        }
    }
}
