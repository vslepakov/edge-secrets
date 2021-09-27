using System.Threading.Tasks;

namespace EdgeSecrets.KeyManagement
{
    public interface IKeyManagementService
    {
        Task<string> DecryptAsync(string ciphertext);

        Task<string> EncryptAsync(string plaintext);

        Task ForgetMeAsync();
    }
}