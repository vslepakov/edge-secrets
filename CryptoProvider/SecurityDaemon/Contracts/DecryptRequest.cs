using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeSecrets.CryptoProvider.SecurityDaemon.Contracts
{
    public class DecryptRequest
    {
        public string InitializationVector { get; set; }

        public string Ciphertext { get; set; }
    }
}
