﻿namespace EdgeSecrets.CryptoProvider
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider.SecurityDaemon;

    public class WorkloadApiCryptoProvider : ICryptoProvider
    {
        private readonly string _initializationVector;
        private readonly string _defaultInitializationVector = "0123456789"; // TO DO: hardcoded for now.  
        readonly SecurityDaemonClient _securityDaemonClient = new SecurityDaemonClient();

        public WorkloadApiCryptoProvider(string initializationVector = null)
        {
            _initializationVector = initializationVector ?? _defaultInitializationVector;
        }

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
