using KeyManagement;
using System;
using System.Threading;
using System.Threading.Tasks;
using EdgeSecrets.SecurityDaemon;

namespace EdgeSecrets.KeyManagement
{
    public class WorkloadApiCryptoProvider : ICryptoProvider
    {
        SecurityDaemonClient _securityDaemonClient = new SecurityDaemonClient();

        public async Task<string> DecryptAsync(string ciphertext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            string plaintext = await _securityDaemonClient.DecryptAsync(ciphertext, "boh");
            return plaintext;
        }

        public async Task<string> EncryptAsync(string plaintext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            
            string ciphertext = await _securityDaemonClient.EncryptAsync(plaintext, "boh");
            return ciphertext;
        }
    }
}
