using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.EventGridEdge.IotEdge
{
    public class DecryptRequest
    {
        public string InitializationVector { get; set; }

        public string Ciphertext { get; set; }
    }
}
