namespace SecretDeliveryApp;

public interface ISecretProvider
{
    Task<Secret> GetSecretAsync(string secretName, string? secretVersion = null);
}