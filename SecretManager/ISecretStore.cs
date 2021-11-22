namespace EdgeSecrets.SecretManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISecretStore
    {
        Task ClearCacheAsync(CancellationToken cancellationToken);

        Task<Secret> GetSecretAsync(string secretName, DateTime date, CancellationToken cancellationToken);

        Task<SecretList> RetrieveSecretsAsync(IList<string> secretNames, CancellationToken cancellationToken);

        Task SetSecretAsync(Secret secret, CancellationToken cancellationToken);
    }
}
