namespace EdgeSecrets.SecretManager
{
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public abstract class SecretStoreBase : ISecretStore
    {
        protected ICryptoProvider _cryptoProvider;
        protected KeyOptions _keyOptions;
        protected ISecretStore _internalSecretStore = default;

        public SecretStoreBase(ICryptoProvider cryptoProvider = null, KeyOptions keyOptions = null, ISecretStore secretStore = null)
        {
            _cryptoProvider = cryptoProvider;
            _keyOptions = keyOptions;
            _internalSecretStore = secretStore;
        }

        protected abstract Task<Secret> GetSecretInternalAsync(string name, CancellationToken cancellationToken);

        public async Task<Secret> GetSecretAsync(string name, CancellationToken cancellationToken)
        {
            Secret value = await GetSecretInternalAsync(name, cancellationToken);
            if (value != null)
            {
                if (_cryptoProvider != null)
                {
                    value.Value = await _cryptoProvider.DecryptAsync(value.Value, _keyOptions);
                }
            }

            // Find secret in cached secret list
            if (value == null)
            {
                // Not found in local file so try to get from delegated secret store
                if (_internalSecretStore != null)
                {
                    value = await _internalSecretStore.GetSecretAsync(name, cancellationToken);
                    await SetSecretInternalAsync(name, value, cancellationToken);
                }
            }

            return value;
        }

        protected abstract Task SetSecretInternalAsync(string name, Secret value, CancellationToken cancellationToken);

        public async Task SetSecretAsync(string name, Secret value, CancellationToken cancellationToken)
        {
            // Store secret into delegated secret store (for example local file)
            if (_internalSecretStore != null)
            {
                await _internalSecretStore.SetSecretAsync(name, value, cancellationToken);
            }

            // Add secret to cached secret list
            if (_cryptoProvider != null)
            {
                value.Value = await _cryptoProvider.EncryptAsync(value.Value, _keyOptions);                
            }
            await SetSecretInternalAsync(name, value, cancellationToken);
        }
    }
}
