using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeSecrets.CryptoProvider.SecurityDaemon.Contracts
{
    public class EncryptRequest
    {
        public string InitializationVector { get; set; }

        public string Plaintext { get; set; }
    }
}
