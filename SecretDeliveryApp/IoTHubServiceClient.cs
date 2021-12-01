using Microsoft.Azure.Devices;

namespace SecretDeliveryApp;

public class IoTHubServiceClient : IIoTHubServiceClient
{
    private const string IOT_HUB_CONNECTION_STRING = "IOT_HUB_CONNECTION_STRING";
    private readonly ServiceClient _serviceClient;
    private readonly ILogger<IoTHubServiceClient> _logger;

    public IoTHubServiceClient(ILogger<IoTHubServiceClient> logger)
    {
        var connString = Environment.GetEnvironmentVariable(IOT_HUB_CONNECTION_STRING);
        _serviceClient = ServiceClient.CreateFromConnectionString(connString);
        _logger = logger;
    }

    public async Task InvokeDeviceMethodAsync(string methodName, string deviceId, string moduleId, string payload, CancellationToken cancellation = default)
    {
        var methodInvocation = new CloudToDeviceMethod(methodName)
        {
            ResponseTimeout = TimeSpan.FromSeconds(10),
        };

        methodInvocation.SetPayloadJson(payload);

        if (string.IsNullOrEmpty(deviceId))
        {
            _logger.LogError("Cannot invoke direct method. DeviceID not specified!");
            return;
        }
        else if (string.IsNullOrEmpty(moduleId))
        {
            var response = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);
            _logger.LogInformation($"Invoked direct method on device: {deviceId} with status: {response.Status} and response: {response.GetPayloadAsJson()}");
        }
        else
        {
            var response = await _serviceClient.InvokeDeviceMethodAsync(deviceId, moduleId, methodInvocation);
            _logger.LogInformation($"Invoked direct method on device {deviceId} and module {moduleId} with status {response.Status} and response: {response.GetPayloadAsJson()}");
        }
    }
}