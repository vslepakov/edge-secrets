using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.EventGridEdge.IotEdge
{
    public class SignRequest
    {
        public string KeyId { get; set; }

        public SignRequestAlgo Algo { get; set; }

        public byte[] Data { get; set; }
    }

    public enum SignRequestAlgo
    {
        [System.Runtime.Serialization.EnumMember(Value = @"HMACSHA256")]
        HMACSHA256 = 0,
    }
}
