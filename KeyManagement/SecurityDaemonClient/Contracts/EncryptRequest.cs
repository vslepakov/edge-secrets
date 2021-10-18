using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeSecrets.SecurityDaemon
{
    public class EncryptRequest
    {
        public string InitializationVector { get; set; }

        public string Plaintext { get; set; }
    }
}
