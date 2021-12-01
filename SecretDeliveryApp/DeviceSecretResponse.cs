namespace SecretDeliveryApp;

public record DeviceSecretResponse(string RequestId, IList<Secret> Secrets);
