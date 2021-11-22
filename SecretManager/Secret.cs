namespace EdgeSecrets.SecretManager
{
    using System;

    public record Secret
    {
        public string Name { get; init; }
        public string Value { get; init; }
        public string Version { get; init; } = default;
        public DateTime ActivationDate { get; init; } = DateTime.MinValue;
        public DateTime ExpirationDate { get; init; } = DateTime.MaxValue;
    }
}
