using System.Threading.Tasks;

namespace KeyManagementService
{
    public interface IKeyManagementService
    {
        Task<string> DecryptAsync(string ciphertext);

        Task<string> EncryptAsync(string plaintext);

        Task ForgetMeAsync();
    }
}