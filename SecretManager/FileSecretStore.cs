namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public class FileSecretStore : SecretStoreBase
    {
        private readonly string _fileName;

        public FileSecretStore(string fileName,
            ISecretStore secretStore = null, ICryptoProvider cryptoProvider = null, KeyOptions keyOptions = null)
            : base(cryptoProvider, keyOptions, secretStore)
        {
            _fileName = fileName;
        }

        protected override async Task ClearCacheInternalAsync(CancellationToken cancellationToken)
        {
            if (File.Exists(_fileName))
            {
                File.Delete(_fileName);
            }
        }

        protected override async Task<Secret> GetSecretInternalAsync(string secretName, DateTime date, CancellationToken cancellationToken)
        {
            SecretList localSecrets = await RetrieveSecretsFromSourceAsync(new List<string>() { secretName }, cancellationToken);
            if (localSecrets != null)
            {
                return localSecrets.GetSecret(secretName, date);
            }
            return null;
        }

        protected override async Task<SecretList> RetrieveSecretsFromSourceAsync(IList<string> secretNames, CancellationToken cancellationToken)
        {
            SecretList localSecrets = null;
            if (File.Exists(_fileName))
            {
                using FileStream openStream = File.OpenRead(_fileName);
                var options = new JsonSerializerOptions { };
                var fileSecrets = await JsonSerializer.DeserializeAsync<SecretList>(openStream, options, cancellationToken);

                if (secretNames != null)
                {
                    localSecrets = new SecretList();
                    foreach (var secretName in secretNames)
                    {
                        if (fileSecrets.ContainsKey(secretName))
                        {
                            localSecrets.Add(secretName, fileSecrets[secretName]);
                        }
                    }
                }
                else
                {
                    localSecrets = fileSecrets;
                }
            }
            return localSecrets;
        }

        protected override async Task SetSecretInternalAsync(Secret secret, CancellationToken cancellationToken)
        {
            // Get secret list from local file (if exists)
            SecretList localSecrets = await RetrieveSecretsFromSourceAsync(null, cancellationToken);
            if (localSecrets == null)
            {
                localSecrets = new SecretList();
            }

            // Add secret to secret list
            localSecrets.SetSecret(secret);

            // Store secret list into local file
            using FileStream createStream = File.Create(_fileName);
            await JsonSerializer.SerializeAsync(createStream, localSecrets);
            await createStream.DisposeAsync();
        }

        protected override async Task MergeSecretsInternalAsync(SecretList secretList, CancellationToken cancellationToken)
        {
            // Get secret list from local file (if exists)
            SecretList localSecrets = await RetrieveSecretsFromSourceAsync(null, cancellationToken);
            if (localSecrets == null)
            {
                localSecrets = new SecretList();
            }

            // Add secret to secret list
            foreach (var secretVersions in secretList.Values)
            {
                foreach (var secret in secretVersions.Values)
                {
                    localSecrets.SetSecret(secret);
                }
            }

            // Store secret list into local file
            using FileStream createStream = File.Create(_fileName);
            await JsonSerializer.SerializeAsync(createStream, localSecrets);
            await createStream.DisposeAsync();
        }
    }
}
