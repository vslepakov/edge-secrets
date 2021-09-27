using System;
using System.Threading.Tasks;

namespace EdgeSecrets.Samples.SecretManager.Common
{
    public interface ISecretStore
    {
        Task<string> GetSecretAsync(string key);

        Task SetSecretAsync(string key, string value);
    }
}
