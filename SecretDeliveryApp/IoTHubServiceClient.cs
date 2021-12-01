using Microsoft.Azure.Devices;

namespace SecretDeliveryApp;

public class IoTHubServiceClient : IIoTHubServiceClient
{
    private readonly ServiceClient _serviceClient;

    public IoTHubServiceClient(ServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public Task InvokeDeviceMethodAsync(string methodName, string payload, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();

        //Console.WriteLine($"Invoking 'Start' dm on device '{this._deviceId}'...");

        //this._runId = runId.ToString();

        //var methodInvocation = new CloudToDeviceMethod("Start")
        //{
        //    ResponseTimeout = TimeSpan.FromSeconds(30),
        //};

        //var payload = new
        //{
        //    runId = runId,
        //    config = config
        //};

        //methodInvocation.SetPayloadJson(JsonConvert.SerializeObject(payload));



        //Console.WriteLine($"Run ID: {runId}");

        //// Invoke the direct method asynchronously and get the response from the simulated device.
        //var response = await this._serviceClient.InvokeDeviceMethodAsync(this._deviceId, this._transmitterModuleName, methodInvocation);

        ////Console.WriteLine($"\nResponse status: {response.Status}, payload: {response.GetPayloadAsJson()}");
    }
}