using Newtonsoft.Json.Linq;

namespace SecretDeliveryApp
{
    public class SecretRequestProcessor
    {
        private readonly ILogger _logger;

        public SecretRequestProcessor(ILogger logger)
        {
            _logger = logger;
        }

        public Task ProcessAsync(string events)
        {
            var message = JToken.Parse(events);

            _logger.LogInformation($"Source: {message["source"]}");
            _logger.LogInformation($"Time: {message["eventTime"]}");
            _logger.LogInformation($"Event data: {message["data"]}");

            return Task.CompletedTask;
        }
    }
}
