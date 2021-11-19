namespace EdgeSecrets.SecretManager.Edge
{
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Exceptions;
    using Microsoft.Azure.Devices.Shared;
    using EdgeSecrets.CryptoProvider;

    public class RemoteSecretStore : SecretStoreBase
    {
        public record PendingRequest
        {
            public RequestSecretRequest Request;
            public TaskCompletionSource<Secret> ResponseReceived = new TaskCompletionSource<Secret>();
        }

        private TransportType _transportType;
        private ClientOptions _clientOptions;
        private ModuleClient _moduleClient = null;
        private Dictionary<string, PendingRequest> _pendingRequests = new Dictionary<string, PendingRequest>();

        public RemoteSecretStore(TransportType transportType, ClientOptions clientOptions = default,
            ICryptoProvider cryptoProvider = null, KeyOptions keyOptions = null, ISecretStore secretStore = null)
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
            Console.WriteLine("Start HandleUpdateSecretsCommand");

            string jsonString = methodRequest.DataAsJson;
            Console.WriteLine($"Received method data '{jsonString}'");
            RequestSecretResponse response = JsonSerializer.Deserialize<RequestSecretResponse>(jsonString);
            Console.WriteLine($"Converted method data into response '{response.RequestId}'");
            if (_pendingRequests.ContainsKey(response.RequestId))
            {
                Console.WriteLine($"This is from a known request");
                var pendingRequest = _pendingRequests[response.RequestId];
                if (pendingRequest != null)
                {
                    int secrets = response.Secrets.Count;
                    Console.WriteLine($"This pending request response had {secrets} secrets.");
                    if (secrets > 0)
                    {
                        pendingRequest.ResponseReceived.TrySetResult(response.Secrets[0]);
                    }
                }
            }
        
            Console.WriteLine("End HandleUpdateSecretsCommand");
            return new MethodResponse(200);
        }        

        protected override async Task<Secret> GetSecretInternalAsync(string name, CancellationToken cancellationToken)
        {
            Secret value = null;

            Console.WriteLine("Start GetSecretAsync");

            await InitializeModuleClient(cancellationToken);

            // Create new request
            var request = new RequestSecretRequest();
            string requestId = request.RequestId;
            request.Secrets.Add(name);

            // Add request to list of pending requests
            var pendingRequest = new PendingRequest() { Request = request };
            _pendingRequests.Add(requestId, pendingRequest);

            // Send the request to the cloud
            var messageString = JsonSerializer.Serialize(request);
            var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));
            message.Properties.Add("secret-request-id", requestId);
            await _moduleClient.SendEventAsync(message, cancellationToken);
            Console.WriteLine($"Send request message with id '{requestId}' to cloud for secret {name}");

            // using (cancellationToken.Register(() => {
            //     // this callback will be executed when token is cancelled
            //     requestSecretResponseReceived.TrySetCanceled();
            // })) {
                value = await pendingRequest.ResponseReceived.Task;
                Console.WriteLine($"Value received back from Method handler for secret {name} = '{value.Value}'");

            // }        
            _pendingRequests.Remove(requestId);

            Console.WriteLine("End GetSecretAsync");
            return value;
        }

        protected override async Task SetSecretInternalAsync(string name, Secret value, CancellationToken cancellationToken)
        {
        }
    }
}