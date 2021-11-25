namespace EdgeSecrets.SecretManager
{
    using System;

    public record class Secret
    {
        public string Name { get; }
        public string? Value { get; init; }
        public string? Version { get; init; } = default;
        public DateTime ActivationDate { get; init; } = DateTime.MinValue;
        public DateTime ExpirationDate { get; init; } = DateTime.MaxValue;

        public Secret(string name, string? value = null)
        {
            Name = name;
            Value = value;
        }

        public bool DateIsActive(DateTime? date)
        {
            return ((date != null) && (date >= ActivationDate) && (date < ExpirationDate));
        }
    }
}
