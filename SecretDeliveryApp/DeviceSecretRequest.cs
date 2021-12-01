namespace SecretDeliveryApp;

public record SecretMetadata(string Name, string? Version);

public record DeviceSecretRequest(string RequestId, DateTime CreateDate, IList<SecretMetadata> Secrets);