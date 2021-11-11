namespace EdgeSecrets.SecretManager
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class FileSecretStore : ISecretStore
    {
        private string _fileName;

        public FileSecretStore(string fileName)
        {
            _fileName = fileName;
        }

        public async Task<Secret> GetSecretAsync(string name)
        {
            if (File.Exists(_fileName))
            {
                IDictionary<string, Secret> secrets;
                using FileStream openStream = File.OpenRead(_fileName);
                secrets = await JsonSerializer.DeserializeAsync<Dictionary<string, Secret>>(openStream);

                if (secrets.TryGetValue(name, out Secret value))
                {
                    return value;
                }
            }
            return null;
        }

        public async Task SetSecretAsync(string key, Secret value)
        {
            IDictionary<string, Secret> secrets;
            if (File.Exists(_fileName))
            {
                using FileStream openStream = File.OpenRead(_fileName);
                secrets = await JsonSerializer.DeserializeAsync<Dictionary<string, Secret>>(openStream);
            }
            else
            {
                secrets = new Dictionary<string, Secret>();
            }

            secrets[key] = value;

            using FileStream createStream = File.Create(_fileName);
            await JsonSerializer.SerializeAsync(createStream, secrets);
            await createStream.DisposeAsync();
        }
    }
}
