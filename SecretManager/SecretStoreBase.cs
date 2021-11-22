namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public abstract class SecretStoreBase : ISecretStore
    {
        private ICryptoProvider _cryptoProvider;
        private KeyOptions _keyOptions;
        private ISecretStore _internalSecretStore = default;

        public SecretStoreBase(ICryptoProvider cryptoProvider = null, KeyOptions keyOptions = null, ISecretStore secretStore = null)
        {
            _cryptoProvider = cryptoProvider;
            _keyOptions = keyOptions;
            _internalSecretStore = secretStore;
        }

        protected abstract Task ClearCacheInternalAsync(CancellationToken cancellationToken);

        public async Task ClearCacheAsync(CancellationToken cancellationToken)
        {
            if (_internalSecretStore != null)
            {
                await _internalSecretStore.ClearCacheAsync(cancellationToken);
            }
            await ClearCacheInternalAsync(cancellationToken);
        }

        protected abstract Task<Secret> GetSecretInternalAsync(string secretName, DateTime date, CancellationToken cancellationToken);

        public async Task<Secret> GetSecretAsync(string secretName, DateTime date, CancellationToken cancellationToken)
        {
            var secret = await GetSecretInternalAsync(secretName, date, cancellationToken);
            if (secret != null)
            {
                if (_cryptoProvider != null)
                {
                    secret = secret with { Value = await _cryptoProvider.DecryptAsync(secret.Value, _keyOptions) };
                }
            }

            // Find secret in cached secret list
            if (secret == null)
            {
                // Not found in local file so try to get from delegated secret store
                if (_internalSecretStore != null)
                {
                    secret = await _internalSecretStore.GetSecretAsync(secretName, date, cancellationToken);
                    if (secret != null)
                    {
                        await SetSecretInternalAsync(secret, cancellationToken);
                    }
                }
            }

            return secret;
        }

        protected abstract Task<SecretList> RetrieveSecretsFromSourceAsync(IList<string> secretNames, CancellationToken cancellationToken);

        public async Task<SecretList> RetrieveSecretsAsync(IList<string> secretNames, CancellationToken cancellationToken)
        {
            SecretList secretList;
            if (_internalSecretStore == null)
            {
                secretList = await RetrieveSecretsFromSourceAsync(secretNames, cancellationToken);
            }
            else
            {
                secretList = await _internalSecretStore.RetrieveSecretsAsync(secretNames, cancellationToken);
                await MergeSecretsInternalAsync(secretList, cancellationToken);
            }
            return secretList;
        }

        protected abstract Task SetSecretInternalAsync(Secret secret, CancellationToken cancellationToken);

        public async Task SetSecretAsync(Secret secret, CancellationToken cancellationToken)
        {
            // Store secret into delegated secret store (for example local file)
            if (_internalSecretStore != null)
            {
                await _internalSecretStore.SetSecretAsync(secret, cancellationToken);
            }

            // Add secret to cached secret list
            if (_cryptoProvider != null)
            {
                secret = secret with { Value = await _cryptoProvider.EncryptAsync(secret.Value, _keyOptions) };
            }
            await SetSecretInternalAsync(secret, cancellationToken);
        }

        protected abstract Task MergeSecretsInternalAsync(SecretList secretList, CancellationToken cancellationToken);
    }
}
