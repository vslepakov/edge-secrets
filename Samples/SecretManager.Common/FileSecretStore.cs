namespace EdgeSecrets.Samples.SecretManager.Common
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

        public async Task<string> GetSecretAsync(string key)
        {
            if (File.Exists(_fileName))
            {
                IDictionary<string, string> secrets;
                using FileStream openStream = File.OpenRead(_fileName);
                secrets = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(openStream);

                if (secrets.TryGetValue(key, out string value))
                {
                    return value;
                }
            }
            return null;
        }

        public async Task SetSecretAsync(string key, string value)
        {
            IDictionary<string, string> secrets;
            if (File.Exists(_fileName))
            {
                using FileStream openStream = File.OpenRead(_fileName);
                secrets = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(openStream);
            }
            else
            {
                secrets = new Dictionary<string, string>();
            }

            secrets[key] = value;

            using FileStream createStream = File.Create(_fileName);
            await JsonSerializer.SerializeAsync(createStream, secrets);
            await createStream.DisposeAsync();
        }
    }
}
