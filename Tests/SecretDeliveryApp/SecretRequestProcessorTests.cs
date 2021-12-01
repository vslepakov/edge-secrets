using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SecretDeliveryApp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.SecretDeliveryApp
{
    public class SecretRequestProcessorTests
    {
        private const string UpdateSecretsDirectMethodName = "UpdateSecrets";
        private const string DeviceId = "LogicAppTestDevice";
        private const string ModuleId = "SecretManager";

        [Fact]
        public async Task Handle_Single_Secrets_Request_Success()
        {
            // Arrange
            const string subject = $"devices/{DeviceId}/modules/{ModuleId}";
            var requestId = Guid.NewGuid().ToString();
            var existingSecret = new Secret("TESTSECRET", "12345", "v1", DateTime.UtcNow.AddDays(30), DateTime.UtcNow);

            // Mock Logger
            var logger = new Mock<ILogger<SecretRequestProcessor>>().Object;

            // Mock Secret Provider
            var secretProvider = CreateSecretProviderMock(existingSecret);

            // Mock IoTHubServiceClient
            var iotHubServiceClient = new Mock<IIoTHubServiceClient>();

            // Setup incoming event
            var body = CreateSecretRequestBody(requestId, DateTime.UtcNow, existingSecret.Name, existingSecret.Version);
            var @event = CreateEvent(requestId, subject, body);

            // Act
            var requestProcessor = new SecretRequestProcessor(logger, secretProvider.Object, iotHubServiceClient.Object);
            await requestProcessor.ProcessAsync(@event);

            // Assert
            var expectedResponse = GetExpectedResponse(requestId.ToString(), new[] { existingSecret });
            iotHubServiceClient.Verify(m => m.InvokeDeviceMethodAsync(
                UpdateSecretsDirectMethodName, DeviceId, ModuleId, expectedResponse, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Handle_Single_Secrets_Request_As_Base64_Success()
        {
            // Arrange
            const string subject = $"devices/{DeviceId}/modules/{ModuleId}";
            var requestId = Guid.NewGuid().ToString();
            var existingSecret = new Secret("TESTSECRET234", "12345", null, DateTime.UtcNow.AddDays(30), DateTime.UtcNow); ;

            // Mock Logger
            var logger = new Mock<ILogger<SecretRequestProcessor>>().Object;

            // Mock Secret Provider
            var secretProvider = CreateSecretProviderMock(existingSecret);

            // Mock IoTHubServiceClient
            var iotHubServiceClient = new Mock<IIoTHubServiceClient>();

            // Setup incoming event
            var body = CreateSecretRequestBodyAsBase64(requestId, DateTime.UtcNow, existingSecret.Name, existingSecret.Version);
            var @event = CreateEvent(requestId, subject, body);

            // Act
            var requestProcessor = new SecretRequestProcessor(logger, secretProvider.Object, iotHubServiceClient.Object);
            await requestProcessor.ProcessAsync(@event);

            // Assert
            var expectedResponse = GetExpectedResponse(requestId.ToString(), new[] { existingSecret });
            iotHubServiceClient.Verify(m => m.InvokeDeviceMethodAsync(
                UpdateSecretsDirectMethodName, DeviceId, ModuleId, expectedResponse, It.IsAny<CancellationToken>()), Times.Once());
        }

        #region Helpers

        private string CreateEvent(string requestId, string subject, string body)
        {
            var properties = new Dictionary<string, string> { { "secret-request-id", requestId } };
            var @event = new
            {
                subject,
                type = "Microsoft.Devices.DeviceTelemetry",
                data = new 
                {
                    body,
                    properties
                }
            };

            return JsonConvert.SerializeObject(@event);
        }

        private string CreateSecretRequestBody(string requestId, DateTime createDate, string secretName, string version)
        {
            var secretMetadata = new SecretMetadata(secretName, version);
            var request = new DeviceSecretRequest(requestId, createDate, new List<SecretMetadata> { secretMetadata });

            return JsonConvert.SerializeObject(@request);
        }

        private string CreateSecretRequestBodyAsBase64(string requestId, DateTime createDate, string secretName, string version)
        {
            var body = CreateSecretRequestBody(requestId, createDate, secretName, version);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(body));
        }

        private string GetExpectedResponse(string requestId, IList<Secret> secrets)
        {
            var response = new DeviceSecretResponse(requestId, secrets);
            return JsonConvert.SerializeObject(response);
        }

        private Mock<ISecretProvider> CreateSecretProviderMock(Secret existingSecret)
        {
            var secretProvider = new Mock<ISecretProvider>();
            secretProvider.Setup(m => m.GetSecretAsync(existingSecret.Name, existingSecret.Version, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(existingSecret));

            return secretProvider;
        }

        #endregion
    }
}
