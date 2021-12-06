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
        private readonly SecretList _cachedSecrets = new();

        public InMemorySecretStore(
            ISecretStore? secretStore, ICryptoProvider? cryptoProvider = null, string? keyId = default)
            : base(secretStore, cryptoProvider, keyId)
        {
        }

        public int LocalSecretCount
        {
            get { return _cachedSecrets.Count; }
        }

        /// <summary>
        /// Clear any cached secrets from the local secret store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ClearCacheInternalAsync(CancellationToken cancellationToken)
        {
            _cachedSecrets.Clear();
            await Task.FromResult(0);
        }

        /// <summary>
        /// Retrieve single secret by name and data from the local secret store.
        /// </summary>
        /// <param name="secretName">Name of the secret to retrieve.</param>
        /// <param name="version">Name of the version to retrieve.</param>
        /// <param name="date">Timestamp where the secret should be valid (between activation and expiration).</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Secret found or null if not found.</returns>
        protected override async Task<Secret?> RetrieveSecretInternalAsync(string secretName, string? version, DateTime? date, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_cachedSecrets.GetSecret(secretName, version, date));
        }

        /// <summary>
        /// Retrieve list of secrets from the local secret store.
        /// </summary>
        /// <param name="secrets">List of secrets to retrieve. Secret should have a name and could have a version.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<SecretList> RetrieveSecretListInternalAsync(IList<Secret> secrets, CancellationToken cancellationToken)
        {
            SecretList localSecretList = new();
            foreach(var secret in secrets)
            {
                var cachedSecret = _cachedSecrets.GetSecret(secret.Name, secret.Version);
                if (cachedSecret != null)
                {
                    localSecretList.SetSecret(cachedSecret);
                }
            }
            return await Task.FromResult<SecretList>(localSecretList);
        }

        /// <summary>
        /// Store single secret in the local secret store.
        /// </summary>
        /// <param name="secret">Secret to store.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task StoreSecretInternalAsync(Secret secret, CancellationToken cancellationToken)
        {
            _cachedSecrets.SetSecret(secret);
            await Task.FromResult(0);
        }

        /// <summary>
        /// Merge secret list into the local secret store.
        /// </summary>
        /// <param name="secretList">Secret list to merge.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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
