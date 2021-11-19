namespace EdgeSecrets.SecretManager.Edge
{
    using System;
    using System.Collections.Generic;

    public class RequestSecretRequest
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreateDate { get; } = DateTime.Now;
        public List<string> Secrets { get; set; } = new List<string>();
    }
}
