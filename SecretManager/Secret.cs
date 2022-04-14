namespace EdgeSecrets.SecretManager
{
    using System;

    public record class Secret
    {
        public string Name { get; }
        public string? Version { get; }
        public string? Value { get; init; }
        public DateTime ActivationDate { get; init; } = DateTime.MinValue;
        public DateTime ExpirationDate { get; init; } = DateTime.MaxValue;

        public Secret(string name, string? version = null, string? value = null)
        {
            Name = name;
            Version = version;
            Value = value;
        }

        public bool DateIsActive(DateTime? date)
        {
            return ((date != null) && (date >= ActivationDate) && (date < ExpirationDate));
        }
    }
}
