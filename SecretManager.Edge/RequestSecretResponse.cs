namespace EdgeSecrets.SecretManager.Edge
{
    using System;
    using System.Collections.Generic;
    using EdgeSecrets.SecretManager;

    public class RequestSecretResponse
    {
        public string RequestId { get; set; }
        public DateTime CreateDate { get; } = DateTime.Now;
        public List<Secret> Secrets { get; set; } = new List<Secret>();
    }
}
