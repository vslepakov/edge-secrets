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
            SecretList? localSecrets = await RetrieveSecretListInternalAsync(new List<Secret?>() { new Secret(secretName, version) }, cancellationToken);
            return localSecrets?.GetSecret(secretName, version, date);
        }

        protected override async Task<SecretList?> RetrieveSecretListInternalAsync(IList<Secret?>? secrets, CancellationToken cancellationToken)
        {
            SecretList? localSecrets = null;
            if (File.Exists(_fileName))
            {
                SecretList? fileSecrets;
                using (FileStream openStream = File.Open(_fileName, FileMode.Open))
                {
                    fileSecrets = await JsonSerializer.DeserializeAsync<SecretList>(openStream, new JsonSerializerOptions(), cancellationToken);
                }

                if ((secrets != null) && (fileSecrets != null))
                {
                    Console.WriteLine($"Get secrets from file {_fileName}, {fileSecrets.Count} secrets found");
                    foreach (var secret in secrets)
                    {
                        if (secret != null)
                        {
                            if (localSecrets == null)
                            {
                                localSecrets = new SecretList();
                            }

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
            SecretList? localSecrets = await RetrieveSecretListInternalAsync(null, cancellationToken);
            if (localSecrets == null)
            {
                localSecrets = new SecretList();
            }

            // Add secret to secret list
            localSecrets.SetSecret(secret);

            // Store secret list into local file
            using (var createStream = File.Create(_fileName))
            {
                await JsonSerializer.SerializeAsync(createStream, localSecrets, cancellationToken: cancellationToken);
                await createStream.DisposeAsync();
                Console.WriteLine($"Add secret to file {_fileName}, file now contains {localSecrets.Count} secrets");
            }
        }

        protected override async Task MergeSecretListInternalAsync(SecretList secretList, CancellationToken cancellationToken)
        {

            // Get secret list from local file (if exists)
            SecretList? localSecrets = await RetrieveSecretListInternalAsync(null, cancellationToken);
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
            using (var createStream = File.Create(_fileName))
            {
                await JsonSerializer.SerializeAsync(createStream, localSecrets, cancellationToken: cancellationToken);
                await createStream.DisposeAsync();
                Console.WriteLine($"Add {secretList.Count} secrets to file {_fileName}, file now contains {localSecrets.Count} secrets");
            }
        }
    }
}
