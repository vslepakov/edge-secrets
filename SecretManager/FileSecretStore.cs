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
            ISecretStore? secretStore = null, ICryptoProvider? cryptoProvider = null, KeyOptions? keyOptions = null)
            : base(secretStore, cryptoProvider, keyOptions)
        {
            _fileName = fileName;
        }

        protected override async Task ClearCacheInternalAsync(CancellationToken cancellationToken)
        {
            if (File.Exists(_fileName))
            {
                File.Delete(_fileName);
            }
            await Task.FromResult(0);
        }

        protected override async Task<Secret?> RetrieveSecretInternalAsync(string secretName, string? version, DateTime? date, CancellationToken cancellationToken)
        {
            SecretList? localSecrets = await RetrieveSecretsFromSourceAsync(new List<Secret?>() { new Secret(secretName, version) }, cancellationToken);
            if (localSecrets != null)
            {
                return localSecrets.GetSecret(secretName, version, date);
            }
            return null;
        }

        protected override async Task<SecretList?> RetrieveSecretsFromSourceAsync(IList<Secret?>? secrets, CancellationToken cancellationToken)
        {
            SecretList? localSecrets = null;
            if (File.Exists(_fileName))
            {
                using FileStream openStream = File.OpenRead(_fileName);
                var options = new JsonSerializerOptions { };
                var fileSecrets = await JsonSerializer.DeserializeAsync<SecretList>(openStream, options, cancellationToken);

                if ((secrets != null) && (fileSecrets != null))
                {
                    localSecrets = new SecretList();
                    foreach (var secret in secrets)
                    {
                        if (secret != null)
                        {
                            // Find secret by name and version
                            if (secret?.Version != null)
                            {
                                Secret? fileSecret = fileSecrets.GetSecret(secret.Name, secret.Version);
                                if (fileSecret != null)
                                {
                                    localSecrets.SetSecret(fileSecret);
                                }
                            }
                            // Find secret by name and add all versions
                            else
                            {
                                if (fileSecrets.ContainsKey(secret!.Name))
                                {
                                    localSecrets.Add(secret.Name, fileSecrets[secret.Name]);
                                }
                            }
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

        protected override async Task StoreSecretInternalAsync(Secret secret, CancellationToken cancellationToken)
        {
            // Get secret list from local file (if exists)
            SecretList? localSecrets = await RetrieveSecretsFromSourceAsync(null, cancellationToken);
            if (localSecrets == null)
            {
                localSecrets = new SecretList();
            }

            // Add secret to secret list
            localSecrets.SetSecret(secret);

            // Store secret list into local file
            using FileStream createStream = File.Create(_fileName);
            await JsonSerializer.SerializeAsync(createStream, localSecrets, cancellationToken: cancellationToken);
            await createStream.DisposeAsync();
        }

        protected override async Task MergeSecretListInternalAsync(SecretList secretList, CancellationToken cancellationToken)
        {
            // Get secret list from local file (if exists)
            SecretList? localSecrets = await RetrieveSecretsFromSourceAsync(null, cancellationToken);
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
            await JsonSerializer.SerializeAsync(createStream, localSecrets, cancellationToken: cancellationToken);
            await createStream.DisposeAsync();
        }
    }
}
