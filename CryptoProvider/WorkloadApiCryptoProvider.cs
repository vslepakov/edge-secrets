namespace EdgeSecrets.CryptoProvider
{
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

        public WorkloadApiCryptoProvider()
        {
            _securityDaemonClient = new SecurityDaemonClient();
        }

        public Task<string> EncryptAsync(string plaintext, string keyId, CancellationToken ct = default)
        {
            return _securityDaemonClient.EncryptAsync(plaintext, _initializationVector);
        }

        public Task<string> DecryptAsync(string ciphertext, string keyId, CancellationToken ct = default)
        {
            return _securityDaemonClient.DecryptAsync(ciphertext, _initializationVector);
        }
    }
}
