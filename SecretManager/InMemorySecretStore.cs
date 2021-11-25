namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider;

    public class InMemorySecretStore : SecretStoreBase
    {
        private readonly SecretList _cachedSecrets = new();

        public InMemorySecretStore(
            ISecretStore? secretStore = null, ICryptoProvider? cryptoProvider = null, KeyOptions? keyOptions = null)
            : base(secretStore, cryptoProvider, keyOptions)
        {
        }

        protected override async Task ClearCacheInternalAsync(CancellationToken cancellationToken)
        {
            _cachedSecrets?.Clear();
            await Task.FromResult(0);
        }

        protected override async Task<Secret?> RetrieveSecretInternalAsync(string secretName, string? version, DateTime? date, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_cachedSecrets?.GetSecret(secretName, version, date));
        }

        protected override async Task<SecretList?> RetrieveSecretsFromSourceAsync(IList<Secret?>? secretNames, CancellationToken cancellationToken)
        {
            return await Task.FromResult<SecretList?>(null);
        }

        protected override async Task StoreSecretInternalAsync(Secret secret, CancellationToken cancellationToken)
        {
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
