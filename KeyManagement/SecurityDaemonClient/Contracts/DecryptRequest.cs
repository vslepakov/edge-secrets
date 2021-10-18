using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeSecrets.SecurityDaemon
{
    public class DecryptRequest
    {
        public string InitializationVector { get; set; }

        public string Ciphertext { get; set; }
    }
}
