namespace SecretDeliveryApp;

public interface IIoTHubServiceClient
{
    Task InvokeDeviceMethodAsync(string methodName, string deviceId, string moduleId, string payload, CancellationToken cancellation = default);
}