namespace EdgeSecrets.SecretManager
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISecretStore
    {
        Task<Secret> GetSecretAsync(string name, CancellationToken cancellationToken);

        Task SetSecretAsync(string name, Secret secret, CancellationToken cancellationToken);
    }
}
