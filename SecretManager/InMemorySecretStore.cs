namespace EdgeSecrets.SecretManager
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public class InMemorySecretStore : SecretStoreBase
    {
        private IDictionary<string, Secret> _cachedSecrets = new Dictionary<string, Secret>();

        public InMemorySecretStore(ICryptoProvider cryptoProvider = null, KeyOptions keyOptions = null, ISecretStore secretStore = null)
            : base(cryptoProvider, keyOptions, secretStore)
        {
        }

        protected override async Task<Secret> GetSecretInternalAsync(string name, CancellationToken cancellationToken)
        {
            Secret value = null;
            _cachedSecrets.TryGetValue(name, out value);
            return value;
        }

        protected override async Task SetSecretInternalAsync(string name, Secret value, CancellationToken cancellationToken)
        {
            _cachedSecrets[name] = value;
        }
    }
}
