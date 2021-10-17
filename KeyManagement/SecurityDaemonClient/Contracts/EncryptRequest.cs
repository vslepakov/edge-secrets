using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.EventGridEdge.IotEdge
{
    public class EncryptRequest
    {
        public string InitializationVector { get; set; }

        public string Plaintext { get; set; }
    }
}
