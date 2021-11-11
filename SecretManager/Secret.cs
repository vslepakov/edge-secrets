namespace EdgeSecrets.SecretManager
{
    using System;

    public record class Secret
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Version { get; set; }
        public DateTime ActivationDate {get; set; } = DateTime.MinValue;
        public DateTime ExpirationDate { get; set; } = DateTime.MaxValue;
    }
}
