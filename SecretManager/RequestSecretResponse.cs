namespace EdgeSecrets.SecretManager
{
    using System.Collections.Generic;
    using EdgeSecrets.SecretManager;

    public record RequestSecretResponse
    {
        public string RequestId { get; init; } = string.Empty;
        public IList<Secret> Secrets { get; init; } = new List<Secret>();
    }
}
