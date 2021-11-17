using Newtonsoft.Json.Linq;

namespace SecretDeliveryApp
{
    public class SecretRequestProcessor
    {
        private readonly ILogger _logger;
        private readonly ISecretProvider _secretProvider;

        public SecretRequestProcessor(ILogger<SecretRequestProcessor> logger, ISecretProvider secretProvider)
        {
            _logger = logger;
            _secretProvider = secretProvider;
        }

        public async Task ProcessAsync(string events)
        {
            var message = JToken.Parse(events);

            // TODO implement real processing!

            if (message is JArray)
            {
                foreach (var item in message)
                {
                    LogDebugInfo(item);
                }
            }
            else if (message is JObject)
            {
                LogDebugInfo(message);
            }

            // Just sample code on how to retrieve a Secret

            var secret = await _secretProvider.GetSecretAsync("daprtest");

            if(secret != NullSecret.Instance)
            {
                _logger.LogInformation("Successfully retrieved secret for Device");
            }
        }

        private void LogDebugInfo(JToken message)
        {
            _logger.LogInformation($"Source: {message["source"]}");
            _logger.LogInformation($"Time: {message["eventTime"]}");
            _logger.LogInformation($"Event data: {message["data"]}");
        }
    }
}
