namespace SecretDeliveryApp;

public record Secret(string Name, string Value, string? Version, DateTime ExpirationDate, DateTime ActivationDate);

public record NullSecret : Secret
{
    private static NullSecret _instance = new("", "", "", DateTime.MaxValue, DateTime.MinValue);

    public NullSecret(string Name, string Value, string Version, DateTime ExpirationDate, DateTime ActivationDate)
        : base(Name, Value, Version, ExpirationDate, ActivationDate)
    {
    }

    public static NullSecret Instance => _instance;
}
