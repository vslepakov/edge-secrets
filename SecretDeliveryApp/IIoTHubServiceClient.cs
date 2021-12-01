namespace SecretDeliveryApp;

public interface IIoTHubServiceClient
{
    Task InvokeDeviceMethodAsync(string methodName, string payload, CancellationToken cancellation = default);
}