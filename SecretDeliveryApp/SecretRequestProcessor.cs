using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace SecretDeliveryApp
{
    public class SecretRequestProcessor
    {
        private const string UpdateSecretsDirectMethodName = "UpdateSecrets";
        private readonly ILogger _logger;
        private readonly ISecretProvider _secretProvider;
        private readonly IIoTHubServiceClient _iotHubServiceClient;

        public SecretRequestProcessor(ILogger<SecretRequestProcessor> logger, 
            ISecretProvider secretProvider, IIoTHubServiceClient iotHubServiceClient)
        {
            _logger = logger;
            _secretProvider = secretProvider;
            _iotHubServiceClient = iotHubServiceClient;
        }

        public async Task ProcessAsync(string events, CancellationToken cancellationToken = default)
        {
            try
            {
                var eventJToken = JToken.Parse(events);
                var eventsToProcess = new List<JToken>();

                if (eventJToken is JArray)
                {
                    eventsToProcess = eventJToken.ToList();
                }
                else if (eventJToken is JObject)
                {
                    eventsToProcess = new List<JToken> { eventJToken };
                }

                foreach (var @event in eventsToProcess)
                {
                    if (IsEventValid(@event))
                    {
                        var request = GetRequestAndSubject(@event);

                        _logger.LogInformation($"Received Secrets Request from subject {request.Item2}");

                        await ProcessRequestForDeviceAsync(request.Item1, request.Item2, cancellationToken);

                        _logger.LogInformation($"Successfully processed Secrets Request for subject {request.Item2}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR {ex.Message} processing Secrets Request");
                throw;
            }
        }

        private async Task ProcessRequestForDeviceAsync(DeviceSecretRequest request, string subject, CancellationToken cancellationToken = default)
        {
            var secrets = new List<Secret>();

            foreach (var secretMetadata in request.Secrets)
            {
                var secret = await _secretProvider.GetSecretAsync(secretMetadata.Name, secretMetadata.Version, cancellationToken);

                if (secret != NullSecret.Instance)
                {
                    secrets.Add(secret);
                }
                else
                {
                    _logger.LogWarning($"Secret with the name {secretMetadata.Name} and version {secretMetadata?.Version} NOT found for subject {subject}");
                }
            }

            var response = new DeviceSecretResponse(request.RequestId, secrets);
            var responseAsJson = JsonConvert.SerializeObject(response);

            var deviceAndModule = GetDeviceAndModuleIdFromSubject(subject);

            await _iotHubServiceClient.InvokeDeviceMethodAsync(UpdateSecretsDirectMethodName, deviceAndModule.Item1, 
                deviceAndModule.Item2, responseAsJson, cancellationToken);
        }

        private bool IsEventValid(JToken jToken)
        {
            return jToken["type"]?.ToString() == "Microsoft.Devices.DeviceTelemetry" && 
                   jToken["data"]!["properties"]!["secret-request-id"] != null;
        }

        private (DeviceSecretRequest, string) GetRequestAndSubject(JToken jToken)
        {
            var body = jToken["data"]!["body"]!.ToString();
            var subject = jToken["subject"]!.ToString();

            if (IsBase64String(body))
            {
                var decodedBody = Convert.FromBase64String(body);
                body = Encoding.UTF8.GetString(decodedBody);
            }

            var request = JsonConvert.DeserializeObject<DeviceSecretRequest>(body);

            return (request!, subject!);
        }

        private (string, string) GetDeviceAndModuleIdFromSubject(string subject)
        {
            var parts = subject.Split('/');

            if (parts.Length == 3)
            {
                return (parts[1], parts[2]);
            }
            else if (parts.Length == 2)
            {
                return (parts[1], string.Empty);
            }
            else
            {
                return (string.Empty, string.Empty);
            }
        }

        public static bool IsBase64String(string base64)
        {
            var buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }
}


