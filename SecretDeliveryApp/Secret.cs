namespace SecretDeliveryApp;

public record Secret(Uri Id, string Name, string Value, string Version, DateTimeOffset? ExpiresOn, DateTimeOffset? NotBefore);

public record NullSecret : Secret
{
    public NullSecret(Uri Id, string Name, string Value, string Version, DateTimeOffset? ExpiresOn, DateTimeOffset? NotBefore)
        : base(Id, Name, Value, Version, ExpiresOn, NotBefore)
    {
    }

    public static NullSecret Instance => new(new Uri("http://localhost"), "", "", "", null, null);
}
