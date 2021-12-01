using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace SecretDeliveryApp;

public class AzureKeyVaultClient : ISecretProvider
{
    private const string AZURE_KEYVAULT_URL_NAME = "AZURE_KEYVAULT_URL";
    private readonly ILogger _logger;
    private readonly SecretClient _secretClient;

    public AzureKeyVaultClient(ILogger<AzureKeyVaultClient> logger)
    {
        _logger = logger;

        var keyVaultUrl = Environment.GetEnvironmentVariable(AZURE_KEYVAULT_URL_NAME) 
            ?? throw new ArgumentException($"Missing KeyVault URL in ENV {AZURE_KEYVAULT_URL_NAME}");

        _secretClient = new SecretClient(new Uri(keyVaultUrl), new EnvironmentCredential());
    }

    public async Task<Secret> GetSecretAsync(string secretName, string? secretVersion = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Getting secret {secretName}!");

        var kvSecret = await _secretClient.GetSecretAsync(secretName, secretVersion);

        if (kvSecret != null)
        {
            var secretValue = kvSecret.Value;
            var secretProps = secretValue.Properties;

            var expirationDate = secretProps.ExpiresOn != null ? secretProps.ExpiresOn.Value.DateTime : DateTime.MaxValue;
            var activationDate = secretProps.NotBefore != null ? secretProps.NotBefore.Value.DateTime : DateTime.MinValue;

            return new Secret(secretValue.Name, secretValue.Value, secretProps.Version, expirationDate, activationDate);
        }
        else
        {
            return NullSecret.Instance;
        }
    }
}