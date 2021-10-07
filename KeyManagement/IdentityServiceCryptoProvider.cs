using KeyManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeSecrets.KeyManagement
{
    public class IdentityServiceCryptoProvider : ICryptoProvider
    {
        private const string ENCRYPT_ENDPOINT = "http://keyd.sock/encrypt?api-version=2021-05-01";
        private const string DECRYPT_ENDPOINT = "http://keyd.sock/decrypt?api-version=2021-05-01";
        private const string KEYD_SOCKET = "/run/aziot/keyd.sock";
        private readonly HttpClient _httpClient;

        public IdentityServiceCryptoProvider()
        {
            _httpClient = HttpClientHelper.GetUnixDomainSocketHttpClient(KEYD_SOCKET, CancellationToken.None);
        }

        public async Task<string> EncryptAsync(string plaintext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            var keyHandle = await GetKeyHandle(keyOptions.KeyId);

            var payload = new
            {
                keyHandle,
                algorithm = "RSA-PKCS1",
                plaintext
            };

            using var request = new HttpRequestMessage(new HttpMethod("POST"), ENCRYPT_ENDPOINT);
            request.Content = new StringContent(JsonConvert.SerializeObject(payload));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await _httpClient.SendAsync(request, ct);
            var json = await response.Content.ReadAsStringAsync(ct);
            var ciphertext = JObject.Parse(json)["ciphertext"].ToString();

            return ciphertext;
        }

        public async Task<string> DecryptAsync(string ciphertext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            var keyHandle = await GetKeyHandle(keyOptions.KeyId);

            var payload = new
            {
                keyHandle,
                algorithm = "RSA-PKCS1",
                ciphertext
            };

            using var request = new HttpRequestMessage(new HttpMethod("POST"), DECRYPT_ENDPOINT);
            request.Content = new StringContent(JsonConvert.SerializeObject(payload));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await _httpClient.SendAsync(request, ct);
            var json = await response.Content.ReadAsStringAsync(ct);
            var plaintext = JObject.Parse(json)["plaintext"].ToString();

            return plaintext;
        }

        private Task<string> GetKeyHandle(string keyId)
        {
            // TODO Find a way to get the keyHandle for the keyId
            return null;
        }
    }
}
