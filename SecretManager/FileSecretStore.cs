namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public static class FileSecretStoreExtensions
    {
        public static SecretManagerClient WithFileSecretStore(this SecretManagerClient client,
            string fileName,
            ICryptoProvider? cryptoProvider = null, string? keyId = default)
        {
            var secretStore = new FileSecretStore(fileName, client.SecretStore, cryptoProvider, keyId);
            client.SecretStore = secretStore;
            return client;
        }
    }

    public class FileSecretStore : SecretStoreBase
    {
        private readonly string _fileName;

        public FileSecretStore(string fileName,
            ISecretStore? secretStore, ICryptoProvider? cryptoProvider = null, string? keyId = default)
            : base(secretStore, cryptoProvider, keyId)
        {
            _fileName = fileName;
        }

        public int LocalSecretCount
        {
            get
            {
                if (File.Exists(_fileName))
                {
                    SecretList? fileSecretList = GetFileSecretList(default).GetAwaiter().GetResult();
                    if (fileSecretList != null)
                    {
                        return fileSecretList.Count;
                    }
                }
                return 0;
            }
        }

        protected async Task<SecretList?> GetFileSecretList(CancellationToken cancellationToken)
        {
            SecretList? fileSecretList;
            using (FileStream openStream = File.Open(_fileName, FileMode.Open))
            {
                fileSecretList = await JsonSerializer.DeserializeAsync<SecretList>(openStream, new JsonSerializerOptions(), cancellationToken);
            }
            return fileSecretList;
        }

        /// <summary>
        /// Clear any cached secrets from the local secret store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ClearCacheInternalAsync(CancellationToken cancellationToken)
        {
            if (File.Exists(_fileName))
            {
                File.Delete(_fileName);
            }
            await Task.FromResult(0);
        }

        /// <summary>
        /// Retrieve single secret by name, version and date from the local secret store.
        /// </summary>
        /// <param name="secretName">Name of the secret to retrieve.</param>
        /// <param name="version">Name of the version to retrieve, or null for first active version.</param>
        /// <param name="date">Date the secret should be valid, or null for any date.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Secret found or null if not found.</returns>
        protected override async Task<Secret?> RetrieveSecretInternalAsync(string secretName, string? version, DateTime? date, CancellationToken cancellationToken)
        {
            SecretList localSecretList = await RetrieveSecretListInternalAsync(new List<string>() { secretName }, cancellationToken);
            return localSecretList.GetSecret(secretName, version, date);
        }

        /// <summary>
        /// Retrieve list of secrets by name from the local secret store.
        /// </summary>
        /// <param name="secretNames">Names of the secrets to retrieve.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<SecretList> RetrieveSecretListInternalAsync(IList<string> secretNames, CancellationToken cancellationToken)
        {
            SecretList localSecretList = new();
            if (File.Exists(_fileName))
            {
                // Read all existing secrets stored in the file
                SecretList? fileSecretList = await GetFileSecretList(cancellationToken);
                if (fileSecretList != null)
                {
                    Console.WriteLine($"Reading secrets from file {_fileName} ({fileSecretList.Count} secrets found)");
                    foreach (var secretName in secretNames)
                    {
                        var fileSecretVersions = fileSecretList.GetSecretVersions(secretName);
                        if (fileSecretVersions != null)
                        {
                            foreach (var fileSecret in fileSecretVersions.Values)
                            {
                                localSecretList.SetSecret(fileSecret);
                            }
                        }
                    }
                }
            }
            return localSecretList;
        }

        /// <summary>
        /// Store single secret in the local secret store.
        /// </summary>
        /// <param name="secret">Secret to store.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task StoreSecretInternalAsync(Secret secret, CancellationToken cancellationToken)
        {
            SecretList localSecretList = new();
            if (File.Exists(_fileName))
            {
                // Read all existing secrets stored in the file
                SecretList? fileSecretList = await GetFileSecretList(cancellationToken);
                if (fileSecretList != null)
                {
                    localSecretList = fileSecretList;
                }
            }

            // Add secret to secret list
            localSecretList.SetSecret(secret);

            // Store secret list into local file
            using (var createStream = File.Create(_fileName))
            {
                await JsonSerializer.SerializeAsync(createStream, localSecretList, cancellationToken: cancellationToken);
                await createStream.DisposeAsync();
                Console.WriteLine($"Add secret to file {_fileName}, file now contains {localSecretList.Count} secrets");
            }
        }

        /// <summary>
        /// Merge secret list into the local secret store.
        /// </summary>
        /// <param name="secretList">Secret list to merge.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task MergeSecretListInternalAsync(SecretList secretList, CancellationToken cancellationToken)
        {
            SecretList localSecretList = new();
            if (File.Exists(_fileName))
            {
                // Read all existing secrets stored in the file
                SecretList? fileSecretList = await GetFileSecretList(cancellationToken);
                if (fileSecretList != null)
                {
                    localSecretList = fileSecretList;
                }
            }

            // Add secret to secret list
            foreach (var secretVersions in secretList.Values)
            {
                foreach (var secret in secretVersions.Values)
                {
                    localSecretList.SetSecret(secret);
                }
            }

            // Store secret list into local file
            using (var createStream = File.Create(_fileName))
            {
                await JsonSerializer.SerializeAsync(createStream, localSecretList, cancellationToken: cancellationToken);
                await createStream.DisposeAsync();
                Console.WriteLine($"Add {secretList.Count} secrets to file {_fileName}, file now contains {localSecretList.Count} secrets");
            }
        }
    }
}
