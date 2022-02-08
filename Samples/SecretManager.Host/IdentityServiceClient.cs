using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SecretManager.Host
{
    internal class IdentityServiceClient
    {
        private const string IdentitySocketAddress = "/run/aziot/identityd.sock";
        private const string KeysSocketAddress = "/run/aziot/keyd.sock";

        private readonly HttpClient _identityClient;
        private readonly HttpClient _keysClient;

        public IdentityServiceClient(IUdsHttpClientFactory udsHttpClientFactory)
        {
            _identityClient = udsHttpClientFactory.CreateHttpClientForSocket(IdentitySocketAddress);
            _keysClient = udsHttpClientFactory.CreateHttpClientForSocket(KeysSocketAddress);
        }

        public async Task<string> GetModuleConnectionStringAsync(long sasTokenLifeTime, CancellationToken cancellationToken = default)
        {
            var identityInfo = await GetIdentityInfoAsync(cancellationToken);

            var dataToSign = GetDataToSign(identityInfo, sasTokenLifeTime);
            var signature = await GetSignatureAsync(identityInfo, dataToSign, cancellationToken);
            var connectionString = GetFullConnectionString(identityInfo, signature, sasTokenLifeTime);

            return connectionString;
        }

        private async Task<IdentityInfo> GetIdentityInfoAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Getting Identity Info...");

            var json = await _identityClient.GetStringAsync(@"http://identityd.sock/identities/identity?api-version=2020-09-01", cancellationToken);

            Console.WriteLine($"Identity Info: {json}");

            var jObject = JObject.Parse(json);
            return JsonConvert.DeserializeObject<IdentityInfo>(jObject["spec"].ToString());
        }

        private string GetDataToSign(IdentityInfo identityInfo, long sasTokenLifeTime)
        {
            var resource_uri = GetResourceUri(identityInfo);

            var signatureData = Encoding.UTF8.GetBytes($"{resource_uri}\n{sasTokenLifeTime}");
            var signatureDataAsBase64 = Convert.ToBase64String(signatureData);
            Console.WriteLine($"Signature Data: {signatureDataAsBase64}");

            return signatureDataAsBase64;
        }

        private async Task<string> GetSignatureAsync(IdentityInfo identityInfo, string dataToSign, CancellationToken cancellationToken)
        {
            Console.WriteLine("Getting Signature from keyd");

            var payload = new
            {
                keyHandle = identityInfo.Auth.KeyHandle,
                algorithm = "HMAC-SHA256",
                parameters = new
                {
                    message = dataToSign
                }
            };

            using var request = new HttpRequestMessage(new HttpMethod("POST"), "http://keyd.sock/sign?api-version=2020-09-01");
            request.Content = new StringContent(JsonConvert.SerializeObject(payload));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await _keysClient.SendAsync(request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var signature = JObject.Parse(json)["signature"];

            Console.WriteLine($"Signature: {signature}");
            return HttpUtility.UrlEncode(signature.ToString());
        }

        private static string GetFullConnectionString(IdentityInfo identityInfo, string signature, long sasTokenLifeTime)
        {
            var sasToken = $"sr={GetResourceUri(identityInfo)}&se={sasTokenLifeTime}&sig={signature}";
            var sas = $"SharedAccessSignature {sasToken}";

            if (identityInfo.HubName == identityInfo.GatewayHost)
            {
                return $"HostName={identityInfo.HubName};DeviceId={identityInfo.DeviceId};ModuleId={identityInfo.ModuleId};SharedAccessSignature={sas}";
            }
            else
            {
                return $"HostName={identityInfo.HubName};DeviceId={identityInfo.DeviceId};ModuleId={identityInfo.ModuleId};SharedAccessSignature={sas};GatewayHost={identityInfo.GatewayHost}";
            }
        }

        private static string GetResourceUri(IdentityInfo identityInfo)
        {
            return HttpUtility.UrlEncode($"{identityInfo.HubName}/devices/{identityInfo.DeviceId}/modules/{identityInfo.ModuleId}");
        }
    }
}
