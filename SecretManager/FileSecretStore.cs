namespace EdgeSecrets.SecretManager
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public class FileSecretStore : SecretStoreBase
    {
        private string _fileName;

        public FileSecretStore(string fileName,
            ICryptoProvider cryptoProvider = null, KeyOptions keyOptions = null, ISecretStore secretStore = null)
            : base(cryptoProvider, keyOptions, secretStore)
        {
            _fileName = fileName;
        }

        protected override async Task<Secret> GetSecretInternalAsync(string name, CancellationToken cancellationToken)
        {
            Secret value = null;
            if (File.Exists(_fileName))
            {
                IDictionary<string, Secret> secrets;
                using FileStream openStream = File.OpenRead(_fileName);
                secrets = await JsonSerializer.DeserializeAsync<Dictionary<string, Secret>>(openStream);
                secrets.TryGetValue(name, out value);
            }
            return value;
        }

        protected override async Task SetSecretInternalAsync(string name, Secret value, CancellationToken cancellationToken)
        {
            // Get secret list from local file (if exists)
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

            // Add secret to secret list
            secrets[name] = value;

            // Store secret list into local file
            using FileStream createStream = File.Create(_fileName);
            await JsonSerializer.SerializeAsync(createStream, secrets);
            await createStream.DisposeAsync();
        }
    }
}
