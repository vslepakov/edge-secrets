using KeyManagement;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventGridEdge.IotEdge;

namespace EdgeSecrets.KeyManagement
{
    public class WorkloadApiCryptoProvider : ICryptoProvider
    {
        SecurityDaemonClient _securityDaemonClient = new SecurityDaemonClient();

        public async Task<string> DecryptAsync(string ciphertext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            string plaintext = await _securityDaemonClient.DecryptAsync(ciphertext, "boh");
            Console.WriteLine($"DecryptAsync - ciphertext:  {ciphertext}");
            Console.WriteLine($"DecryptAsync - plaintext:   {plaintext}");
            return plaintext;
        }

        public async Task<string> EncryptAsync(string plaintext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            
            string ciphertext = await _securityDaemonClient.EncryptAsync(plaintext, "boh");
            Console.WriteLine($"EncryptAsync - plaintext:   {plaintext}");
            Console.WriteLine($"EncryptAsync - ciphertext:  {ciphertext}");
            return ciphertext;
        }
    }
}
