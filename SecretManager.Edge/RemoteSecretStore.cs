namespace EdgeSecrets.SecretManager.Edge
{
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using EdgeSecrets.CryptoProvider;

    public class RemoteSecretStore : SecretStoreBase
    {
        public record PendingRequest
        {
            public RequestSecretRequest Request;
            public TaskCompletionSource<RequestSecretResponse> ResponseReceived = new();
        }

        private TransportType _transportType;
        private ClientOptions _clientOptions;
        private ModuleClient _moduleClient = null;
        private Dictionary<string, PendingRequest> _pendingRequests = new Dictionary<string, PendingRequest>();

        public RemoteSecretStore(TransportType transportType, ClientOptions clientOptions = default,
            ISecretStore secretStore = null, ICryptoProvider cryptoProvider = null, KeyOptions keyOptions = null)
            : base(cryptoProvider, keyOptions, secretStore)
        {
            _transportType = transportType;
            _clientOptions = clientOptions;
        }

        protected async Task InitializeModuleClient(CancellationToken cancellationToken)
        {
            if (_moduleClient == null)
            {
                _moduleClient = await ModuleClient.CreateFromEnvironmentAsync(_transportType, _clientOptions);
                await _moduleClient.SetMethodHandlerAsync("UpdateSecrets", HandleUpdateSecretsCommand, this, cancellationToken);
            }
        }

        private async Task<MethodResponse> HandleUpdateSecretsCommand(MethodRequest methodRequest, object userContext)
        {
            var response = JsonSerializer.Deserialize<RequestSecretResponse>(methodRequest.DataAsJson);

            // Find pending request with the same RequestId as the response
            if (_pendingRequests.TryGetValue(response.RequestId, out PendingRequest request))
            {
                Console.WriteLine($"Received update of secrets for RequestId '{response.RequestId}'");

                // Complete wait Task with the Response as result
                request.ResponseReceived.TrySetResult(response);
            }
            else
            {
                Console.WriteLine($"Received update of secrets for unknown Request id '{response.RequestId}'");
            }

            return new MethodResponse(200);
        }

        protected override async Task ClearCacheInternalAsync(CancellationToken cancellationToken)
        {
        }

        protected override async Task<Secret> GetSecretInternalAsync(string secretName, DateTime date, CancellationToken cancellationToken)
        {
            SecretList secretList = await RetrieveSecretsFromSourceAsync(new List<string>() { secretName }, cancellationToken);
            return secretList.GetSecret(secretName, date);
        }

        protected override async Task<SecretList> RetrieveSecretsFromSourceAsync(IList<string> secretNames, CancellationToken cancellationToken)
        {
            await InitializeModuleClient(cancellationToken);

            // Create new request
            var request = new RequestSecretRequest() {  Secrets = secretNames };

            // Add request to list of pending requests
            var pendingRequest = new PendingRequest() { Request = request };
            _pendingRequests.Add(request.RequestId, pendingRequest);

            // Send the request to the cloud
            var messageString = JsonSerializer.Serialize(request);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));
            message.Properties.Add("secret-request-id", request.RequestId);
            await _moduleClient.SendEventAsync(message, cancellationToken);
            Console.WriteLine($"Send request for secrets with id '{request.RequestId}'");

            // Wait for the response
            IList<Secret> secrets = null;
            using (cancellationToken.Register(() => {
                pendingRequest.ResponseReceived.TrySetCanceled();
            })) {
                RequestSecretResponse response = await pendingRequest.ResponseReceived.Task;
                secrets = response.Secrets;
            }

            // Remove the request from the list of pending requests
            _pendingRequests.Remove(request.RequestId);

            // Convert secrets to secret list
            return new SecretList(secrets);
        }

        protected override async Task SetSecretInternalAsync(Secret value, CancellationToken cancellationToken)
        {
        }

        protected override async Task MergeSecretsInternalAsync(SecretList secretList, CancellationToken cancellationToken)
        {
        }
    }
}