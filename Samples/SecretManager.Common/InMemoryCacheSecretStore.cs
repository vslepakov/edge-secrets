using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdgeSecrets.Samples.SecretManager.Common
{
    public class InMemoryCacheSecretStore : ISecretStore
    {
        private ISecretStore _internalSecretStore;
        private IDictionary<string, string> _cachedSecrets = new Dictionary<string, string>();


        public InMemoryCacheSecretStore(ISecretStore secretStore)
        {
            _internalSecretStore = secretStore;
        }

        public async Task<string> GetSecretAsync(string key)
        {
            string value;
            if (!_cachedSecrets.TryGetValue(key, out value))
            {
                value = await _internalSecretStore?.GetSecretAsync(key);
                _cachedSecrets[key] = value;
            }
            return value;
        }

        public async Task SetSecretAsync(string key, string value)
        {
            await _internalSecretStore?.SetSecretAsync(key, value);
            _cachedSecrets[key] = value;
        }
    }
}
