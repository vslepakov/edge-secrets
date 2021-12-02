namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public static class InMemorySecretStoreExtensions
    {
        public static SecretManagerClient WithInMemorySecretStore(this SecretManagerClient client,
            ICryptoProvider? cryptoProvider = null, string? keyId = default)
        {
            var secretStore = new InMemorySecretStore(client.SecretStore, cryptoProvider, keyId);
            client.SecretStore = secretStore;
            return client;
        }
    }

    public class InMemorySecretStore : SecretStoreBase
    {
        private SecretList? _cachedSecrets = null;

        public InMemorySecretStore(
            ICryptoProvider? cryptoProvider = null, string? keyId = default)
            : this(null, cryptoProvider, keyId)
        {
        }

        public InMemorySecretStore(
            ISecretStore? secretStore, ICryptoProvider? cryptoProvider = null, string? keyId = default)
            : base(secretStore, cryptoProvider, keyId)
        {
        }

        protected override async Task ClearCacheInternalAsync(CancellationToken cancellationToken)
        {
            _cachedSecrets = null;
            await Task.FromResult(0);
        }

        protected override async Task<Secret?> RetrieveSecretInternalAsync(string secretName, string? version, DateTime? date, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_cachedSecrets?.GetSecret(secretName, version, date));
        }

        protected override async Task<SecretList?> RetrieveSecretListInternalAsync(IList<Secret?>? secrets, CancellationToken cancellationToken)
        {
            SecretList? localSecrets = null;
            if ((secrets != null) && (_cachedSecrets != null))
            {
                foreach(var secret in secrets)
                {
                    if (secret != null)
                    {
                        var cachedSecret = _cachedSecrets?.GetSecret(secret.Name, secret.Version);
                        if (cachedSecret != null)
                        {
                            if (localSecrets == null)
                            {
                                localSecrets = new SecretList();
                            }
                            localSecrets.SetSecret(cachedSecret);
                        }
                    }
                }
            }
            else
            {
                localSecrets = _cachedSecrets;
            }
            return await Task.FromResult<SecretList?>(localSecrets);
        }

        protected override async Task StoreSecretInternalAsync(Secret secret, CancellationToken cancellationToken)
        {
            if (_cachedSecrets == null)
            {
                _cachedSecrets = new();
            }
            _cachedSecrets?.SetSecret(secret);
            await Task.FromResult(0);
        }

        protected override async Task MergeSecretListInternalAsync(SecretList secretList, CancellationToken cancellationToken)
        {
            foreach (var secretVersions in secretList.Values)
            {
                foreach (var secret in secretVersions.Values)
                {
                    await StoreSecretInternalAsync(secret, cancellationToken);
                }
            }
        }
    }
}
