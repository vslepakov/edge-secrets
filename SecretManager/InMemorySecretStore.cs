namespace EdgeSecrets.SecretManager
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class InMemorySecretStore : ISecretStore
    {
        private ISecretStore _internalSecretStore;
        private IDictionary<string, string> _cachedSecrets = new Dictionary<string, string>();


        public InMemorySecretStore(ISecretStore secretStore = null)
        {
            _internalSecretStore = secretStore;
        }

        public async Task<string> GetSecretAsync(string key)
        {
            string value = null;
            if (!_cachedSecrets.TryGetValue(key, out value))
            {
                if (_internalSecretStore != null)
                {
                    value = await _internalSecretStore.GetSecretAsync(key);
                    _cachedSecrets[key] = value;
                }
            }
            return value;
        }

        public async Task SetSecretAsync(string key, string value)
        {
            if (_internalSecretStore != null)
            {
                await _internalSecretStore.SetSecretAsync(key, value);
            }
            _cachedSecrets[key] = value;
        }
    }
}
