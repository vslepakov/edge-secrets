namespace EdgeSecrets.CryptoProvider
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider.Exceptions;
    using global::CryptoProvider.Util;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class IdentityServiceCryptoProvider : ICryptoProvider
    {
        private const string ENCRYPT_ENDPOINT = "http://keyd.sock/encrypt?api-version=2020-09-01";
        private const string DECRYPT_ENDPOINT = "http://keyd.sock/decrypt?api-version=2020-09-01";
        private const string GET_ASYMMETRIC_KEYHANDLE_ENDPOINT = "http://keyd.sock/keypair/{0}?api-version=2020-09-01";
        private const string GET_SYMMETRIC_KEYHANDLE_ENDPOINT = "http://keyd.sock/key/{0}?api-version=2020-09-01";
        private const string KEYD_SOCKET = "/run/aziot/keyd.sock";
        private const string SYMMETRIC_ALGORITHM = "AEAD";
        private const string ASYMMETRIC_ALGORITHM = "RSA-PKCS1";
        private const int RSA_PKCS1_PADDING_SIZE_IN_BYTES = 11;

        private readonly HttpClient _httpClient;

        public IdentityServiceCryptoProvider()
        {
            _httpClient = Util.HttpClientHelper.GetUnixDomainSocketHttpClient(KEYD_SOCKET, CancellationToken.None);
        }

        public Task<string> EncryptAsync(string plaintext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            return keyOptions.KeyType switch
            {
                KeyType.RSA => InternalEncryptAsync(plaintext, keyOptions, ct),
                KeyType.Symmetric => InternalEncryptAsync(plaintext, keyOptions, ct),
                KeyType.ECC => throw new NotImplementedException(),
                _ => throw new ArgumentException($"{keyOptions.KeyType} is not supported by this provider"),
            };
        }

        public Task<string> DecryptAsync(string ciphertext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            return keyOptions.KeyType switch
            {
                KeyType.RSA => InternalDecryptAsync(ciphertext, keyOptions, ct),
                KeyType.Symmetric => InternalDecryptAsync(ciphertext, keyOptions, ct),
                KeyType.ECC => throw new NotImplementedException(),
                _ => throw new ArgumentException($"{keyOptions.KeyType} is not supported by this provider"),
            };
        }

        private async Task<string> InternalEncryptAsync(string plaintext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            string keyHandle;
            object payload;

            if (keyOptions.KeyType == KeyType.RSA)
            {
                if (plaintextBytes.Length + RSA_PKCS1_PADDING_SIZE_IN_BYTES > keyOptions.KeySize / 8)
                {
                    throw new DataTooLargeException($"Data too large to encrypt using {ASYMMETRIC_ALGORITHM} with the key size {keyOptions.KeySize}");
                }

                keyHandle = await GetKeyHandle(keyOptions, GET_ASYMMETRIC_KEYHANDLE_ENDPOINT, ct);
                payload = new { keyHandle, algorithm = ASYMMETRIC_ALGORITHM, plaintext = Convert.ToBase64String(plaintextBytes) };
            }
            else if (keyOptions.KeyType == KeyType.Symmetric)
            {
                keyHandle = await GetKeyHandle(keyOptions, GET_SYMMETRIC_KEYHANDLE_ENDPOINT, ct);
                payload = new
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
            }
            else
            {
                throw new ArgumentException($"Unsupported KeyType {keyOptions.KeyType}");
            }

            var json = await SendRequestAsync(payload, ENCRYPT_ENDPOINT, ct);
            return JObject.Parse(json)["ciphertext"].ToString();
        }

        private async Task<string> InternalDecryptAsync(string ciphertext, KeyOptions keyOptions, CancellationToken ct = default)
        {
            var ciphertextBytes = Convert.FromBase64String(ciphertext);
            string keyHandle;
            object payload;

            if (keyOptions.KeyType == KeyType.RSA)
            {
                if (ciphertextBytes.Length > keyOptions.KeySize / 8)
                {
                    throw new DataTooLargeException($"Data too large to decrypt using {ASYMMETRIC_ALGORITHM} with the key size {keyOptions.KeySize}");
                }

                keyHandle = await GetKeyHandle(keyOptions, GET_ASYMMETRIC_KEYHANDLE_ENDPOINT, ct);
                payload = new { keyHandle, algorithm = ASYMMETRIC_ALGORITHM, ciphertext };
            }
            else if (keyOptions.KeyType == KeyType.Symmetric)
            {
                keyHandle = await GetKeyHandle(keyOptions, GET_SYMMETRIC_KEYHANDLE_ENDPOINT, ct);
                payload = new
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
            }
            else
            {
                throw new ArgumentException($"Unsupported KeyType {keyOptions.KeyType}");
            }

            var json = await SendRequestAsync(payload, DECRYPT_ENDPOINT, ct);
            var plaintextAsBase64 = JObject.Parse(json)["plaintext"].ToString();

            return plaintextAsBase64.Base64Decode();
        }

        private async Task<string> GetKeyHandle(KeyOptions keyOptions, string ednpoint, CancellationToken ct = default)
        {
            var response = await _httpClient.GetAsync(string.Format(ednpoint, keyOptions.KeyId), ct);
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
