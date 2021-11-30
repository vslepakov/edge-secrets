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
            public RequestSecretRequest? Request;
            public TaskCompletionSource<RequestSecretResponse> ResponseReceived = new();
        }

        private int _timeout = 10000; // 10s
        private TransportType _transportType;
        private ClientOptions? _clientOptions;
        private ModuleClient? _moduleClient = null;
        private Dictionary<string, PendingRequest> _pendingRequests = new();

        public RemoteSecretStore(TransportType transportType, ClientOptions? clientOptions = default,
            ISecretStore? secretStore = null, ICryptoProvider? cryptoProvider = null, KeyOptions? keyOptions = null)
            : base(secretStore, cryptoProvider, keyOptions)
        {
            _transportType = transportType;
            _clientOptions = clientOptions;
        }

        #region ModuleClient handling
        protected async Task<bool> InitializeModuleClient(CancellationToken cancellationToken)
        {
            if (_moduleClient == null)
            {
                _moduleClient = await ModuleClient.CreateFromEnvironmentAsync(_transportType, _clientOptions);
                if (_moduleClient != null)
                {
                    await _moduleClient.SetMethodHandlerAsync("UpdateSecrets", HandleUpdateSecretsCommand, this, cancellationToken);
                    return true;
                }
            }
            return false;
        }

        private async Task<MethodResponse> HandleUpdateSecretsCommand(MethodRequest methodRequest, object userContext)
        {
            var response = JsonSerializer.Deserialize<RequestSecretResponse>(methodRequest.DataAsJson);

            // Find pending request with the same RequestId as the response
            if (response != null)
            {
                if (_pendingRequests.TryGetValue(response.RequestId, out PendingRequest? request))
                {
                    // Complete wait Task with the Response as result
                    request?.ResponseReceived.SetResult(response);
                }
                else
                {
                    Console.WriteLine($"Received update of secrets for unknown Request id '{response.RequestId}'");
                }
            }

            return await Task.FromResult(new MethodResponse(200));
        }
        #endregion

        protected override async Task ClearCacheInternalAsync(CancellationToken cancellationToken)
        {
            await Task.FromResult(0);
        }

        protected override async Task<Secret?> RetrieveSecretInternalAsync(string secretName, string? version, DateTime? date, CancellationToken cancellationToken)
        {
            SecretList? localSecrets = await RetrieveSecretListInternalAsync(new List<Secret?>() { new Secret(secretName) }, cancellationToken);
            return localSecrets?.GetSecret(secretName, version, date);
        }

        protected override async Task<SecretList?> RetrieveSecretListInternalAsync(IList<Secret?>? secrets, CancellationToken cancellationToken)
        {
            if (await InitializeModuleClient(cancellationToken))
            {
                // Create new request
                var request = new RequestSecretRequest() { Secrets = secrets };

                // Add request to list of pending requests
                var pendingRequest = new PendingRequest() { Request = request };
                _pendingRequests.Add(request.RequestId, pendingRequest);

                // Send the request to the cloud
                var messageString = JsonSerializer.Serialize(request);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("secret-request-id", request.RequestId);
                await _moduleClient!.SendEventAsync(message, cancellationToken);
                Console.WriteLine($"Send request for secrets with id '{request.RequestId}'");

                // Wait for the response
                IList<Secret?>? remoteSecrets = null;
                try
                {
                    using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                    {
                        cts.CancelAfter(_timeout);
                        using (cts.Token.Register(() => pendingRequest.ResponseReceived.SetCanceled(), useSynchronizationContext: false))
                        {
                            var response = await pendingRequest.ResponseReceived.Task.ConfigureAwait(continueOnCapturedContext: false);
                            Console.WriteLine($"Received update of secrets for RequestId '{response?.RequestId}' ({response?.Secrets?.Count} secret(s) received)");
                            remoteSecrets = response?.Secrets;
                        }                        
                    }
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"RetrieveSecretsFromSourceAsync timed out.");
                }  

                // Remove the request from the list of pending requests
                _pendingRequests.Remove(request.RequestId);

                // Convert secrets to secret list
                if (remoteSecrets != null)
                {
                    return new SecretList(remoteSecrets);
                }
            }
            return null;
        }

        protected override async Task StoreSecretInternalAsync(Secret value, CancellationToken cancellationToken)
        {
            await Task.FromResult(0);
        }

        protected override async Task MergeSecretListInternalAsync(SecretList secretList, CancellationToken cancellationToken)
        {
            await Task.FromResult(0);
        }
    }
}