﻿namespace EdgeSecrets.CryptoProvider
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EdgeSecrets.CryptoProvider.Exceptions;
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
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            if (keyOptions.KeyType != KeyType.Symmetric)
            {
                if (plaintextBytes.Length + RSA_PKCS1_PADDING_SIZE_IN_BYTES > keyOptions.KeySize / 8)
                {
                    throw new DataTooLargeException($"Data too large to encrypt using {ASYMMETRIC_ALGORITHM} with the key size {keyOptions.KeySize}");
                }
            }

            var keyHandle = await GetKeyHandle(keyOptions, ct);
            var payload = new { keyHandle, algorithm, plaintext = Convert.ToBase64String(plaintextBytes) };

            var json = await SendRequestAsync(payload, ENCRYPT_ENDPOINT, ct);
            Console.WriteLine($"Encrypt json: {json}");

            var ciphertextAsBase64 = JObject.Parse(json)["ciphertext"].ToString();
            return ciphertextAsBase64;
            //var ciphertextAsBase64EncodedBytes = Convert.FromBase64String(ciphertextAsBase64);
            //return Encoding.UTF8.GetString(ciphertextAsBase64EncodedBytes);
        }

        private async Task<string> DecryptAsync(string ciphertext, KeyOptions keyOptions, string algorithm, CancellationToken ct = default)
        {
            var ciphertextBytes = Convert.FromBase64String(ciphertext);

            if (keyOptions.KeyType != KeyType.Symmetric)
            {
                if (ciphertextBytes.Length > keyOptions.KeySize / 8)
                {
                    throw new DataTooLargeException($"Data too large to decrypt using {ASYMMETRIC_ALGORITHM} with the key size {keyOptions.KeySize}");
                }
            }

            var keyHandle = await GetKeyHandle(keyOptions, ct);
            var payload = new { keyHandle, algorithm, ciphertext };

            var json = await SendRequestAsync(payload, DECRYPT_ENDPOINT, ct);
            var plaintextAsBase64 = JObject.Parse(json)["plaintext"].ToString();
            var plaintextAsBase64EncodedBytes = Convert.FromBase64String(plaintextAsBase64);

            return Encoding.UTF8.GetString(plaintextAsBase64EncodedBytes);
        }

        private async Task<string> GetKeyHandle(KeyOptions keyOptions, CancellationToken ct = default)
        {
            var response = (keyOptions.KeyType == KeyType.RSA || keyOptions.KeyType == KeyType.ECC)
                ? await _httpClient.GetAsync(string.Format(GET_ASYMMETRIC_KEYHANDLE_ENDPOINT, keyOptions.KeyId), ct)
                : await _httpClient.GetAsync(string.Format(GET_SYMMETRIC_KEYHANDLE_ENDPOINT, keyOptions.KeyId), ct);

            var json = await response.Content.ReadAsStringAsync(ct);
            var keyHandle = JObject.Parse(json)["keyHandle"].ToString();

            return keyHandle;
        }

        private async Task<string> SendRequestAsync(object payload, string endpoint, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(new HttpMethod("POST"), endpoint);
            request.Content = new StringContent(JsonConvert.SerializeObject(payload));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await _httpClient.SendAsync(request, ct);
            Console.WriteLine($"Response: {response.StatusCode}");

            return await response.Content.ReadAsStringAsync(ct);
        }
    }
}
