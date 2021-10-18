using KeyManagement;
using System;
using System.Threading;
using System.Threading.Tasks;
using EdgeSecrets.SecurityDaemon;

namespace EdgeSecrets.KeyManagement
{
    public class WorkloadApiCryptoProvider : ICryptoProvider
    {
        const string _initializationVector="init"; // TEMP
        SecurityDaemonClient _securityDaemonClient = new SecurityDaemonClient();

        public async Task<string> DecryptAsync(string ciphertext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            return keyOptions.KeyType switch
            {
                KeyType.RSA => throw new NotImplementedException(),
                KeyType.ECC => throw new NotImplementedException(),
                KeyType.Symmetric => await _securityDaemonClient.DecryptAsync(ciphertext, _initializationVector),
                _ => throw new ArgumentException($"{keyOptions.KeyType} is not supported by this provider"),
            };
        }

        public async Task<string> EncryptAsync(string plaintext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            return keyOptions.KeyType switch
            {
                KeyType.RSA => throw new NotImplementedException(),
                KeyType.ECC => throw new NotImplementedException(),
                KeyType.Symmetric => await _securityDaemonClient.EncryptAsync(plaintext, _initializationVector),
                _ => throw new ArgumentException($"{keyOptions.KeyType} is not supported by this provider"),
            };
        }
    }
}
