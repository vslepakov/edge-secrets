using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SecretDeliveryApp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.SecretDeliveryApp
{
    public class SecretRequestProcessorTests
    {
        private const string UpdateSecretsDirectMethodName = "UpdateSecrets";

        [Fact]
        public async Task Handle_Single_Secrets_Request_Success()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var existingSecret = new Secret("TESTSECRET", "12345", "v1", DateTime.UtcNow.AddDays(30), DateTime.UtcNow);

            // Mock Logger
            var logger = new Mock<ILogger<SecretRequestProcessor>>().Object;

            // Mock Secret Provider
            var secretProvider = new Mock<ISecretProvider>();
            secretProvider.Setup(m => m.GetSecretAsync(existingSecret.Name, existingSecret.Version, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(existingSecret));

            // Mock IoTHubServiceClient
            var iotHubServiceClient = new Mock<IIoTHubServiceClient>();

            // Act
            var @event = CreateEvent(requestId.ToString(), DateTime.UtcNow, existingSecret.Name, existingSecret.Version);
            var requestProcessor = new SecretRequestProcessor(logger, secretProvider.Object, iotHubServiceClient.Object);
            await requestProcessor.ProcessAsync(@event);

            // Assert
            var expectedResponse = GetExpectedResponse(requestId.ToString(), new[] { existingSecret });
            iotHubServiceClient.Verify(m => m.InvokeDeviceMethodAsync(UpdateSecretsDirectMethodName, expectedResponse, It.IsAny<CancellationToken>()), Times.Once());
        }

        #region Helpers

        private string CreateEvent(string requestId, DateTime createDate, string secretName, string version)
        {
            var properties = new Dictionary<string, string> { { "secret-request-id", requestId } };
            var secretMetadata = new SecretMetadata(secretName, version);
            var request = new DeviceSecretRequest(requestId, createDate, new List<SecretMetadata> { secretMetadata });
            var @event = new
            {
                subject = "devices/LogicAppTestDevice",
                type = "Microsoft.Devices.DeviceTelemetry",
                data = new 
                {
                    body = request 
                },
                properties
            };

            return JsonConvert.SerializeObject(@event);
        }

        private string GetExpectedResponse(string requestId, IList<Secret> secrets)
        {
            var response = new DeviceSecretResponse(requestId, secrets);
            return JsonConvert.SerializeObject(response);
        }

        #endregion
    }
}
