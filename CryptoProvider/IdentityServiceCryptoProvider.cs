namespace EdgeSecrets.CryptoProvider
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using global::CryptoProvider.Util;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class IdentityServiceCryptoProvider : ICryptoProvider
    {
        private const string ENCRYPT_ENDPOINT = "http://keyd.sock/encrypt?api-version=2020-09-01";
        private const string DECRYPT_ENDPOINT = "http://keyd.sock/decrypt?api-version=2020-09-01";
        private const string GET_KEYHANDLE_ENDPOINT = "http://keyd.sock/key/{0}?api-version=2020-09-01";
        private const string KEYD_SOCKET = "/run/aziot/keyd.sock";
        private const string SYMMETRIC_ALGORITHM = "AEAD";

        private readonly HttpClient _httpClient;

        public IdentityServiceCryptoProvider()
        {
            _httpClient = Util.HttpClientHelper.GetUnixDomainSocketHttpClient(KEYD_SOCKET, CancellationToken.None);
        }

        public async Task<string> EncryptAsync(string plaintext, string keyId, CancellationToken ct = default)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            var keyHandle = await GetKeyHandle(keyId, ct);

            var payload = new
            {
                keyHandle,
                algorithm = SYMMETRIC_ALGORITHM,
                plaintext = Convert.ToBase64String(plaintextBytes),
                parameters = new
                {
                    iv = "TEST".Base64Encode(),
                    aad = "TEST".Base64Encode()
                }
            };

            var json = await SendRequestAsync(payload, ENCRYPT_ENDPOINT, ct);
            return JObject.Parse(json)["ciphertext"].ToString();
        }

        public async Task<string> DecryptAsync(string ciphertext, string keyId, CancellationToken ct = default)
        {
            var ciphertextBytes = Convert.FromBase64String(ciphertext);

            var keyHandle = await GetKeyHandle(keyId, ct);

            var payload = new
            {
                keyHandle,
                algorithm = SYMMETRIC_ALGORITHM,
                ciphertext = Convert.ToBase64String(ciphertextBytes),
                parameters = new
                {
                    iv = "TEST".Base64Encode(),
                    aad = "TEST".Base64Decode()
                }
            };

            var json = await SendRequestAsync(payload, DECRYPT_ENDPOINT, ct);
            var plaintextAsBase64 = JObject.Parse(json)["plaintext"].ToString();

            return plaintextAsBase64.Base64Decode();
        }

        private async Task<string> GetKeyHandle(string keyId, CancellationToken ct = default)
        {
            var response = await _httpClient.GetAsync(string.Format(GET_KEYHANDLE_ENDPOINT, keyId), ct);
            var json = await response.Content.ReadAsStringAsync(ct);
            return JObject.Parse(json)["keyHandle"].ToString();
        }

        private async Task<string> SendRequestAsync(object payload, string endpoint, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(new HttpMethod("POST"), endpoint);
            request.Content = new StringContent(JsonConvert.SerializeObject(payload));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await _httpClient.SendAsync(request, ct);
            return await response.Content.ReadAsStringAsync(ct);
        }
    }
}
