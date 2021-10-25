namespace EdgeSecrets.CryptoProvider
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class IdentityServiceCryptoProvider : ICryptoProvider
    {
        private const string ENCRYPT_ENDPOINT = "http://keyd.sock/encrypt?api-version=2021-05-01";
        private const string DECRYPT_ENDPOINT = "http://keyd.sock/decrypt?api-version=2021-05-01";
        private const string KEYD_SOCKET = "/run/aziot/keyd.sock";
        private const string SYMMETRIC_ALGORITHM = "AEAD";
        private const string ASYMMETRIC_ALGORITHM = "RSA-PKCS1";

        private readonly HttpClient _httpClient;

        public IdentityServiceCryptoProvider()
        {
            _httpClient = Util.HttpClientHelper.GetUnixDomainSocketHttpClient(KEYD_SOCKET, CancellationToken.None);
        }

        public Task<string> EncryptAsync(string plaintext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            return keyOptions.KeyType switch
            {
                KeyType.RSA => EncryptAsync(plaintext, keyOptions, ASYMMETRIC_ALGORITHM, ct),
                KeyType.Symmetric => EncryptAsync(plaintext, keyOptions, SYMMETRIC_ALGORITHM, ct),
                KeyType.ECC => throw new NotImplementedException(),
                _ => throw new ArgumentException($"{keyOptions.KeyType} is not supported by this provider"),
            };
        }

        public Task<string> DecryptAsync(string ciphertext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            return keyOptions.KeyType switch
            {
                KeyType.RSA => DecryptAsync(ciphertext, keyOptions, ASYMMETRIC_ALGORITHM, ct),
                KeyType.Symmetric => DecryptAsync(ciphertext, keyOptions, SYMMETRIC_ALGORITHM, ct),
                KeyType.ECC => throw new NotImplementedException(),
                _ => throw new ArgumentException($"{keyOptions.KeyType} is not supported by this provider"),
            };
        }

        private async Task<string> EncryptAsync(string plaintext, KeyOptions keyOptions, string algorithm, CancellationToken ct = default)
        {
            var keyHandle = await GetKeyHandle(keyOptions.KeyId);

            var payload = new
            {
                keyHandle,
                algorithm,
                plaintext
            };

            var json = await SendRequestAsync(payload, ENCRYPT_ENDPOINT, ct);
            var ciphertext = JObject.Parse(json)["ciphertext"].ToString();

            return ciphertext;
        }

        private async Task<string> DecryptAsync(string ciphertext, KeyOptions keyOptions, string algorithm, CancellationToken ct = default)
        {
            var keyHandle = await GetKeyHandle(keyOptions.KeyId);

            var payload = new
            {
                keyHandle,
                algorithm,
                ciphertext
            };

            var json = await SendRequestAsync(payload, DECRYPT_ENDPOINT, ct);
            var plaintext = JObject.Parse(json)["plaintext"].ToString();

            return plaintext;
        }

        private Task<string> GetKeyHandle(string keyId)
        {
            // TODO Find a way to get the keyHandle for the keyId
            return null;
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
