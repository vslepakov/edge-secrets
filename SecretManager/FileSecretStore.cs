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
                Console.WriteLine($"==>FileSecretStore:RetrieveSecretsFromSourceAsync file '{_fileName}' exists");
                SecretList? fileSecrets;
                using (FileStream openStream = File.Open(_fileName, FileMode.Open))
                {
                    fileSecrets = await JsonSerializer.DeserializeAsync<SecretList>(openStream, new JsonSerializerOptions(), cancellationToken);
                }

                if ((secrets != null) && (fileSecrets != null))
                {
                    Console.WriteLine($"==>FileSecretStore:RetrieveSecretsFromSourceAsync file contains {fileSecrets.Count} secrets");
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
                                    Console.WriteLine($"==>FileSecretStore:RetrieveSecretsFromSourceAsync secret '{secret.Name}' of version {fileSecret.Version} has value {fileSecret.Value}");
                                }
                            }
                            // Find secret by name and add all versions
                            else
                            {
                                if (fileSecrets.ContainsKey(secret!.Name))
                                {
                                    localSecrets.Add(secret.Name, fileSecrets[secret.Name]);
                                    Console.WriteLine($"==>FileSecretStore:RetrieveSecretsFromSourceAsync secret '{secret.Name}' contains {fileSecrets[secret.Name].Count} versions");
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
            Console.WriteLine($"==>FileSecretStore:StoreSecretInternalAsync store secret '{secret.Name}' with value '{secret.Value}' locally");

            // Get secret list from local file (if exists)
            SecretList? localSecrets = await RetrieveSecretListInternalAsync(null, cancellationToken);
            if (localSecrets == null)
            {
                localSecrets = new SecretList();
            }

            // Add secret to secret list
            localSecrets.SetSecret(secret);
            Console.WriteLine($"==>FileSecretStore:StoreSecretInternalAsync secret '{secret.Name}' stored in temp local store (contains {localSecrets.Count} secret(s))");

            // Store secret list into local file
            using (var createStream = File.Create(_fileName))
            {
                await JsonSerializer.SerializeAsync(createStream, localSecrets, cancellationToken: cancellationToken);
                await createStream.DisposeAsync();
                Console.WriteLine($"==>FileSecretStore:StoreSecretInternalAsync file '{_fileName}' stored");
            }
        }

        protected override async Task MergeSecretListInternalAsync(SecretList secretList, CancellationToken cancellationToken)
        {
            Console.WriteLine($"==>FileSecretStore:MergeSecretListInternalAsync merge secrets to file secrets");

            // Get secret list from local file (if exists)
            SecretList? localSecrets = await RetrieveSecretListInternalAsync(null, cancellationToken);
            if (localSecrets == null)
            {
                localSecrets = new SecretList();
                Console.WriteLine($"==>FileSecretStore:MergeSecretListInternalAsync no file secrets found, so start with empty list");
            }

            // Add secret to secret list
            foreach (var secretVersions in secretList.Values)
            {
                foreach (var secret in secretVersions.Values)
                {
                    localSecrets.SetSecret(secret);
                    Console.WriteLine($"==>FileSecretStore:MergeSecretListInternalAsync merge secret '{secret.Name}' of version '{secret.Version}' and value '{secret.Value}'");
                }
            }

            // Store secret list into local file
            using (var createStream = File.Create(_fileName))
            {
                await JsonSerializer.SerializeAsync(createStream, localSecrets, cancellationToken: cancellationToken);
                await createStream.DisposeAsync();
                Console.WriteLine($"==>FileSecretStore:MergeSecretListInternalAsync file '{_fileName}' stored");
            }
        }
    }
}
