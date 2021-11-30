namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public abstract class SecretStoreBase : ISecretStore
    {
        private readonly ICryptoProvider? _cryptoProvider;
        private readonly KeyOptions? _keyOptions;
        private readonly ISecretStore? _internalSecretStore;

        public SecretStoreBase(
            ISecretStore? secretStore = null, ICryptoProvider ? cryptoProvider = null, KeyOptions? keyOptions = null)
        {
            _cryptoProvider = cryptoProvider;
            _keyOptions = keyOptions;
            _internalSecretStore = secretStore;
        }

        /// <summary>
        /// Clear any cached secrets from the implemented secret store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task ClearCacheInternalAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Clear any cached secrets from the implemented or internal secret store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ClearCacheAsync(CancellationToken cancellationToken)
        {
            if (_internalSecretStore != null)
            {
                await _internalSecretStore.ClearCacheAsync(cancellationToken);
            }
            await ClearCacheInternalAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieve single secret by name and data from the implemented secret store.
        /// </summary>
        /// <param name="secretName">Name of the secret to retrieve.</param>
        /// <param name="version">Name of the version to retrieve.</param>
        /// <param name="date">Timestamp where the secret should be valid (between activation and expiration).</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Secret found or null if not found.</returns>
        protected abstract Task<Secret?> RetrieveSecretInternalAsync(string secretName, string? version, DateTime? date, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieve single secret by name and date from the implemented or internal secret store.
        /// Decrypt the value if any crypto provider is available.
        /// </summary>
        /// <param name="secretName">Name of the secret to retrieve.</param>
        /// <param name="version">Name of the version to retrieve.</param>
        /// <param name="date">Timestamp where the secret should be valid (between activation and expiration).</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Secret found (with decrypted value) or null if not found.</returns>
        public async Task<Secret?> RetrieveSecretAsync(string secretName, string? version, DateTime? date, CancellationToken cancellationToken)
        {
            var secret = await RetrieveSecretInternalAsync(secretName, version, date, cancellationToken);
            if (secret != null)
            {
                Console.WriteLine($"==>SecretStoreBase:RetrieveSecretAsync secret '{secretName}' found locally with value '{secret.Value}'");
                if (_cryptoProvider != null)
                {
                    secret = secret with { Value = await _cryptoProvider.DecryptAsync(secret.Value, _keyOptions, cancellationToken) };
                    Console.WriteLine($"==>SecretStoreBase:RetrieveSecretAsync decrypted secret to value = '{secret.Value}'");
                }
            }
            else
            {
                Console.WriteLine($"==>SecretStoreBase:RetrieveSecretAsync secret '{secretName}' not found locally");
                if (_internalSecretStore != null)
                {
                    secret = await _internalSecretStore.RetrieveSecretAsync(secretName, version, date, cancellationToken);
                    if (secret != null)
                    {
                        Console.WriteLine($"==>SecretStoreBase:RetrieveSecretAsync secret '{secretName}' with value '{secret.Value}' retrieved from source and stored locally");
                        Secret? storeSecret;
                        if (_cryptoProvider != null)
                        {
                            Console.WriteLine($"==>SecretStoreBase:RetrieveSecretAsync encrypt secret value '{secret.Value}'");
                            storeSecret = secret with { Value = await _cryptoProvider.EncryptAsync(secret.Value, _keyOptions, cancellationToken) };
                            Console.WriteLine($"==>SecretStoreBase:RetrieveSecretAsync encrypted value = '{storeSecret.Value}'");
                        }
                        else
                        {
                            storeSecret = secret;
                        }
                        await StoreSecretInternalAsync(storeSecret, cancellationToken);
                    }
                }
            }

            return secret;
        }

        /// <summary>
        /// Retrieve list of secrets from the implemented secret store.
        /// This method will only retrieve as is, not update or store any secret in local cache and no encrypt/decrypt.
        /// </summary>
        /// <param name="secrets">List of secrets to retrieve. Secret should have a name and could have a version.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<SecretList?> RetrieveSecretListInternalAsync(IList<Secret?>? secrets, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieve list of secrets by name from the implemented or internal secret store.
        /// </summary>
        /// <param name="secrets">List of secrets to retrieve.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SecretList?> RetrieveSecretListAsync(IList<Secret?>? secrets, CancellationToken cancellationToken)
        {
            Console.WriteLine($"==>SecretStoreBase:RetrieveSecretListAsync begin");
            SecretList? secretList = await RetrieveSecretListInternalAsync(secrets, cancellationToken);
            if (secretList != null)
            {
                Console.WriteLine($"==>SecretStoreBase:RetrieveSecretListAsync secrets found locally");
                if (_cryptoProvider != null)
                {
                    SecretList? decryptedSecretList = new();
                    foreach (var secretVersions in secretList.Values)
                    {
                        foreach (var secret in secretVersions.Values)
                        {
                            var decryptedSecret = secret with { Value = await _cryptoProvider.DecryptAsync(secret.Value, _keyOptions, cancellationToken) };
                            decryptedSecretList.SetSecret(decryptedSecret);
                        }
                    }
                    secretList = decryptedSecretList;
                    Console.WriteLine($"==>SecretStoreBase:RetrieveSecretListAsync decrypted secrets");
                }
            }
            else
            {
                Console.WriteLine($"==>SecretStoreBase:RetrieveSecretListAsync secrets not found locally");
                if (_internalSecretStore != null)
                {
                    secretList = await _internalSecretStore.RetrieveSecretListAsync(secrets, cancellationToken);
                    if (secretList != null)
                    {
                        SecretList? storeSecretList;
                        if (_cryptoProvider != null)
                        {
                            storeSecretList = new();
                            foreach (var secretVersions in secretList.Values)
                            {
                                foreach (var secret in secretVersions.Values)
                                {
                                    var encryptedSecret = secret with { Value = await _cryptoProvider.EncryptAsync(secret.Value, _keyOptions, cancellationToken) };
                                    storeSecretList.SetSecret(encryptedSecret);
                                }
                            }
                            Console.WriteLine($"==>SecretStoreBase:RetrieveSecretListAsync encrypted secrets store locally");
                        }
                        else
                        {
                            storeSecretList = secretList;
                        }

                        // encrypt
                        await MergeSecretListInternalAsync(storeSecretList, cancellationToken);
                        Console.WriteLine($"==>SecretStoreBase:RetrieveSecretListAsync secrets retrieved from source and merged locally");
                    }
                }
            }
            return secretList;
        }

        /// <summary>
        /// Store single secret in the implemented secret store.
        /// </summary>
        /// <param name="secret">Secret to store.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task StoreSecretInternalAsync(Secret secret, CancellationToken cancellationToken);

        /// <summary>
        /// Store single secret in the implemented or internal secret store.
        /// Encrypt the value if any crypto provider is available.
        /// </summary>
        /// <param name="secret">Secret to store.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StoreSecretAsync(Secret secret, CancellationToken cancellationToken)
        {
            // Store secret into delegated secret store (for example local file)
            if (_internalSecretStore != null)
            {
                await _internalSecretStore.StoreSecretAsync(secret, cancellationToken);
            }

            // Add secret to cached secret list
            if (_cryptoProvider != null)
            {
                Console.WriteLine($"==>SecretStoreBase:StoreSecretAsync encrypt secret value '{secret.Value}'");
                secret = secret with { Value = await _cryptoProvider.EncryptAsync(secret.Value, _keyOptions, cancellationToken) };
                Console.WriteLine($"==>SecretStoreBase:StoreSecretAsync encrypted value = '{secret.Value}'");
            }
            await StoreSecretInternalAsync(secret, cancellationToken);
        }

        /// <summary>
        /// Merge secret list into the implemented secret store.
        /// </summary>
        /// <param name="secretList">Secret list to merge.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task MergeSecretListInternalAsync(SecretList secretList, CancellationToken cancellationToken);
    }
}
