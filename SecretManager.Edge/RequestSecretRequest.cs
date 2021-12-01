namespace EdgeSecrets.SecretManager.Edge
{
    using System;
    using System.Collections.Generic;

    public record RequestSecretRequest
    {
        public string RequestId { get; init; } = Guid.NewGuid().ToString();
        public DateTime CreateDate { get; } = DateTime.Now;
        public IList<Secret?>? Secrets { get; init; } = new List<Secret?>();
    }
}
