namespace EdgeSecrets.SecretManager
{
    using System.Threading.Tasks;

    public interface ISecretStore
    {
        Task<Secret> GetSecretAsync(string name);

        Task SetSecretAsync(string name, Secret secret);
    }
}
