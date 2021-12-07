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
        private readonly string? _keyId;
        private readonly ISecretStore? _externalSecretStore;

        public SecretStoreBase(
            ISecretStore? secretStore = null, ICryptoProvider? cryptoProvider = null, string? keyId = default)
        {
            _cryptoProvider = cryptoProvider;
            _keyId = keyId;
            _externalSecretStore = secretStore;
        }

        private bool IsSource { get { return _externalSecretStore == null; } }

        /// <summary>
        /// Clear any cached secrets from the local secret store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task ClearCacheInternalAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Clear any cached secrets from the local or external secret store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ClearCacheAsync(CancellationToken cancellationToken)
        {
            if (_externalSecretStore != null)
            {
                await _externalSecretStore.ClearCacheAsync(cancellationToken);
            }
            await ClearCacheInternalAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieve single secret by name and date from the local secret store.
        /// </summary>
        /// <param name="secretName">Name of the secret to retrieve.</param>
        /// <param name="version">Name of the version to retrieve.</param>
        /// <param name="date">Timestamp where the secret should be valid (between activation and expiration).</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Secret found or null if not found.</returns>
        protected abstract Task<Secret?> RetrieveSecretInternalAsync(string secretName, string? version, DateTime? date, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieve single secret by name and date from the local secret store, or external secret store if not found locally.
        /// Decrypt the value of the secret from the local store if any crypto provider is available.
        /// </summary>
        /// <param name="secretName">Name of the secret to retrieve.</param>
        /// <param name="version">Name of the version to retrieve.</param>
        /// <param name="date">Timestamp where the secret should be valid (between activation and expiration).</param>
        /// <param name="forceRetrieve">Force the retrieval of the secret from the deepest secret store source, otherwise cached values can be retured.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Secret found (with decrypted value) or null if not found.</returns>
        public async Task<Secret?> RetrieveSecretAsync(string secretName, string? version, DateTime? date, bool forceRetrieve, CancellationToken cancellationToken)
        {
            Secret? secret = null;

            // Get secret from local secret store (if retrieve from source is not forced) or if the local secret store is the source
            if (!forceRetrieve || IsSource)
            {
                secret = await RetrieveSecretInternalAsync(secretName, version, date, cancellationToken);

                // If the secret was found, decrypt the value of the secret (if crypto provider is available)
                if (secret != null)
                {
                    if ((_cryptoProvider != null) && (secret.Value != null))
                    {
                        secret = secret with { Value = await _cryptoProvider.DecryptAsync(secret.Value, _keyId, cancellationToken) };
                    }
                }
            }

            // Secret not found in local secret store, so reach out to external secret store
            if (secret == null)
            {
                if (_externalSecretStore != null)
                {
                    secret = await _externalSecretStore.RetrieveSecretAsync(secretName, version, date, forceRetrieve, cancellationToken);
                    if (secret != null)
                    {
                        // Store the secret in the local secret store with an encrypt value (if crypto provider is available)
                        Secret storeSecret = secret;
                        if ((_cryptoProvider != null) && (secret.Value != null))
                        {
                            storeSecret = secret with { Value = await _cryptoProvider.EncryptAsync(secret.Value, _keyId, cancellationToken) };
                        }
                        await StoreSecretInternalAsync(storeSecret, cancellationToken);
                    }
                }
            }

            return secret;
        }

        /// <summary>
        /// Retrieve list of secrets from the local secret store.
        /// </summary>
        /// <param name="secrets">List of secrets to retrieve. Secret should have a name and could have a version.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<SecretList> RetrieveSecretListInternalAsync(IList<Secret> secrets, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieve list of secrets by name from the local secret store, or external secret store if not found.
        /// Decrypt the value of the secrets from the local store if any crypto provider is available.
        /// </summary>
        /// <param name="secrets">List of secrets to retrieve.</param>
        /// <param name="forceRetrieve">Force the retrieval of the secret from the deepest secret store source, otherwise cached values can be retured.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SecretList> RetrieveSecretListAsync(IList<Secret> secrets, bool forceRetrieve, CancellationToken cancellationToken)
        {
            SecretList localSecretList = new();
            List<Secret> notFoundSecrets = new();

            // Get secrets from local secret store (if retrieve from source is not forced) or if the local secret store is the source
            if (!forceRetrieve || IsSource)
            {
                SecretList internalSecretList = await RetrieveSecretListInternalAsync(secrets, cancellationToken);
                foreach(var secret in secrets)
                {
                    // If secret to retrieve is found in local secret store, add to list of found secrets
                    var internalSecret = internalSecretList.GetSecret(secret.Name, secret.Version);
                    if (internalSecret != null)
                    {
                        Secret foundSecret = internalSecret;
                        if ((_cryptoProvider != null) && (internalSecret.Value != null))
                        {
                            foundSecret = internalSecret with { Value = await _cryptoProvider.DecryptAsync(internalSecret.Value, _keyId, cancellationToken) };
                        }
                        localSecretList.SetSecret(foundSecret);
                    }
                    // Not found in local secret store, so add to list ofnot found secrets
                    else
                    {
                        notFoundSecrets.Add(secret);
                    }
                }
            }

            // If not all secrets have been retrieved, retrieve the other secrets from the external secret store
            if (notFoundSecrets.Count > 0)
            {
                if (_externalSecretStore != null)
                {
                    SecretList externalSecretList = await _externalSecretStore.RetrieveSecretListAsync(notFoundSecrets, forceRetrieve, cancellationToken);
                    foreach(var secret in notFoundSecrets)
                    {
                        var externalSecret = externalSecretList.GetSecret(secret.Name, secret.Version);
                        if (externalSecret != null)
                        {
                            // Store the secret in the local secret store with an encrypt value (if crypto provider is available)
                            Secret storeSecret = externalSecret;
                            if ((_cryptoProvider != null) && (externalSecret.Value != null))
                            {
                                storeSecret = externalSecret with { Value = await _cryptoProvider.EncryptAsync(externalSecret.Value, _keyId, cancellationToken) };
                            }
                            await StoreSecretInternalAsync(storeSecret, cancellationToken);

                            // Add the secret to list of found secrets
                            localSecretList.SetSecret(externalSecret);
                        }
                        else
                        {
                            Console.WriteLine($"Secret '{secret.Name}' and version '{secret.Version}' not found in local and external secret store");
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
        protected abstract Task StoreSecretInternalAsync(Secret secret, CancellationToken cancellationToken);

        /// <summary>
        /// Store single secret in the local secret store and external secret store if available.
        /// Encrypt the value of the secret if any crypto provider is available.
        /// </summary>
        /// <param name="secret">Secret to store.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StoreSecretAsync(Secret secret, CancellationToken cancellationToken)
        {
            // Store secret into delegated secret store (for example local file)
            if (_externalSecretStore != null)
            {
                await _externalSecretStore.StoreSecretAsync(secret, cancellationToken);
            }

            // Add secret to cached secret list
            if (_cryptoProvider != null)
            {
                secret = secret with { Value = await _cryptoProvider.EncryptAsync(secret.Value, _keyId, cancellationToken) };
            }
            await StoreSecretInternalAsync(secret, cancellationToken);
        }

        /// <summary>
        /// Merge secret list into the local secret store.
        /// </summary>
        /// <param name="secretList">Secret list to merge.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task MergeSecretListInternalAsync(SecretList secretList, CancellationToken cancellationToken);
    }
}
