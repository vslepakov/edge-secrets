namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class SecretManagerClient
    {
        public ISecretStore? SecretStore { get; set; }

        /// <summary>
        /// Create a new SecretManagerClient using the given SecretStore to store the secrets.
        /// Please note that a fluent interface exists to chain multiple SecretStores together.
        /// </summary>
        /// <param name="secretStore">To store the secrets.</param>
        public SecretManagerClient(ISecretStore? secretStore = null)
        {
            SecretStore = secretStore;
        }

        /// <summary>
        /// Clear the cache of all the chained secret stores.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ClearCacheAsync(CancellationToken cancellationToken = default)
        {
            if (SecretStore != null)
            {
                await SecretStore.ClearCacheAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Get a single secret from the chained secret stores.
        /// </summary>
        /// <param name="name">Name of the secret.</param>
        /// <param name="version">Version of the secret, or null for first active version.</param>
        /// <param name="date">Date the secret should be valid, or null for any date.</param>
        /// <param name="forceRetrieve">Force the retrieval of the secret from the deepest secret store source, otherwise cached values can be retured.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Secret?> GetSecretAsync(string name, string? version, DateTime? date, bool forceRetrieve = false, CancellationToken cancellationToken = default)
        {
            if (SecretStore != null)
            {
                return await SecretStore.RetrieveSecretAsync(name, version, date, forceRetrieve, cancellationToken);
            }
            return null;
        }

        /// <summary>
        /// Get the value of a single secret from the chained secret stores.
        /// </summary>
        /// <param name="name">Name of the secret.</param>
        /// <param name="version">Version of the secret, or null for first active version.</param>
        /// <param name="date">Date the secret should be valid, or null for any date.</param>
        /// <param name="forceRetrieve">Force the retrieval of the secret from the deepest secret store source, otherwise cached values can be retured.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string?> GetSecretValueAsync(string name, string? version, DateTime? date, bool forceRetrieve = false, CancellationToken cancellationToken = default)
        {
            if (SecretStore != null)
            {
                var secret = await SecretStore.RetrieveSecretAsync(name, version, date, forceRetrieve, cancellationToken);
                return secret?.Value;
            }
            return null;
        }

        /// <summary>
        /// Get all secret versions for a list of secret names from the chained secret stores.
        /// All versions of the secret will be retrieved.
        /// </summary>
        /// <param name="secretNames">Names of the secrets to retrieve.</param>
        /// <param name="forceRetrieve">Force the retrieval of the secret from the deepest secret store source, otherwise cached values can be retured.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SecretList?> GetSecretListAsync(IList<string> secretNames, bool forceRetrieve = false, CancellationToken cancellationToken = default)
        {
            if (SecretStore != null)
            {
                return await SecretStore.RetrieveSecretListAsync(secretNames, forceRetrieve, cancellationToken);
            }
            return null;
        }

        /// <summary>
        /// Set the values of the given secret.
        /// </summary>
        /// <param name="secret">Secret to set.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetSecretAsync(Secret secret, CancellationToken cancellationToken = default)
        {
            if ((SecretStore != null) && (secret.Version != null))
            {
                await SecretStore.StoreSecretAsync(secret, cancellationToken);
            }
        }

        /// <summary>
        /// Set the values of a specific secret and version.
        /// </summary>
        /// <param name="name">Name of the secret to set.</param>
        /// <param name="version">Version of the secret to set.</param>
        /// <param name="value">Value to set the secret to.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetSecretValueAsync(string name, string version, string value, CancellationToken cancellationToken = default)
        {
            Secret secret = new Secret(name, version, value);
            await SetSecretAsync(secret, cancellationToken);
        }
    }
}
