namespace EdgeSecrets.SecretManager
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class InMemorySecretStore : ISecretStore
    {
        private ISecretStore _internalSecretStore;
        private IDictionary<string, Secret> _cachedSecrets = new Dictionary<string, Secret>();


        public InMemorySecretStore(ISecretStore secretStore = null)
        {
            _internalSecretStore = secretStore;
        }

        public async Task<Secret> GetSecretAsync(string name)
        {
            Secret value = null;
            if (!_cachedSecrets.TryGetValue(name, out value))
            {
                if (_internalSecretStore != null)
                {
                    value = await _internalSecretStore.GetSecretAsync(name);
                    _cachedSecrets[name] = value;
                }
            }
            return value;
        }

        public async Task SetSecretAsync(string name, Secret value)
        {
            if (_internalSecretStore != null)
            {
                await _internalSecretStore.SetSecretAsync(name, value);
            }
            _cachedSecrets[name] = value;
        }
    }
}
