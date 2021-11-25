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
                if (_cryptoProvider != null)
                {
                    secret = secret with { Value = await _cryptoProvider.DecryptAsync(secret.Value, _keyOptions, cancellationToken) };
                }
            }

            // Not found in implemented store so try to get from delegated secret store
            else
            {
                if (_internalSecretStore != null)
                {
                    secret = await _internalSecretStore.RetrieveSecretAsync(secretName, version, date, cancellationToken);
                    if (secret != null)
                    {
                        await StoreSecretInternalAsync(secret, cancellationToken);
                    }
                }
            }

            return secret;
        }

        /// <summary>
        /// Retrieve list of secrets from the implemented secret store.
        /// </summary>
        /// <param name="secrets">List of secrets to retrieve. Secret should have a name and could have a version.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<SecretList?> RetrieveSecretsFromSourceAsync(IList<Secret?>? secrets, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieve list of secrets by name from the implemented or internal secret store.
        /// </summary>
        /// <param name="secretNames">List of secret names to retrieve.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SecretList?> RetrieveSecretsAsync(IList<Secret?>? secrets, CancellationToken cancellationToken)
        {
            SecretList? secretList;
            if (_internalSecretStore == null)
            {
                secretList = await RetrieveSecretsFromSourceAsync(secrets, cancellationToken);
            }
            else
            {
                secretList = await _internalSecretStore.RetrieveSecretsAsync(secrets, cancellationToken);
                if (secretList != null)
                {
                    await MergeSecretListInternalAsync(secretList, cancellationToken);
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
                secret = secret with { Value = await _cryptoProvider.EncryptAsync(secret.Value, _keyOptions, cancellationToken) };
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
