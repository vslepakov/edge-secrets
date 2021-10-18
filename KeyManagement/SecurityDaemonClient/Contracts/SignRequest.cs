using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeSecrets.SecurityDaemon
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
