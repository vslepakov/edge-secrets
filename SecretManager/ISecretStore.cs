namespace EdgeSecrets.SecretManager
{
    using System.Threading.Tasks;

    public interface ISecretStore
    {
        Task<string> GetSecretAsync(string key);

        Task SetSecretAsync(string key, string value);
    }
}
