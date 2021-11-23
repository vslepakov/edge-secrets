namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISecretStore
    {
        Task ClearCacheAsync(CancellationToken cancellationToken);

        Task<Secret> RetrieveSecretAsync(string secretName, DateTime date, CancellationToken cancellationToken);

        Task<SecretList> RetrieveSecretsAsync(IList<string> secretNames, CancellationToken cancellationToken);

        Task StoreSecretAsync(Secret secret, CancellationToken cancellationToken);
    }
}
