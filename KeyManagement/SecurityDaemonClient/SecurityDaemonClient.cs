// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace EdgeSecrets.SecurityDaemon
{
    // [Obsolete("This is a pubternal API that's being made public as a stop-gap measure. It will be removed from the Event Grid SDK nuget package as soon IoT Edge SDK ships with a built-in a security daemon client.")]
    public sealed class SecurityDaemonClient : IDisposable
    {
        private const string UnixScheme = "unix";
        private const int DefaultServerCertificateValidityInDays = 90;
        private const int DefaultIdentityCertificateValidityInDays = 7;
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter() },
        };

        private readonly string moduleGenerationId;
        private readonly string edgeGatewayHostName;
        private readonly string workloadApiVersion;

        private readonly HttpClient httpClient;
        private readonly Uri getTrustBundleUri;
        private readonly Uri postIdentityCertificateRequestUri;
        private readonly Uri postServerCertificateRequestUri;
        private readonly Uri postSignRequestUri;
        private readonly Uri postEncryptRequestUri;
        private readonly Uri postDecryptRequestUri;
        private readonly string asString;

        public SecurityDaemonClient()
        {
            this.ModuleId = Environment.GetEnvironmentVariable("IOTEDGE_MODULEID");
            this.DeviceId = Environment.GetEnvironmentVariable("IOTEDGE_DEVICEID");
            this.IotHubHostName = Environment.GetEnvironmentVariable("IOTEDGE_IOTHUBHOSTNAME");
           
            this.moduleGenerationId = Environment.GetEnvironmentVariable("IOTEDGE_MODULEGENERATIONID");
            this.edgeGatewayHostName = Environment.GetEnvironmentVariable("IOTEDGE_GATEWAYHOSTNAME");
            this.workloadApiVersion = Environment.GetEnvironmentVariable("IOTEDGE_APIVERSION");
            string workloadUriString = Environment.GetEnvironmentVariable("IOTEDGE_WORKLOADURI");

            Validate.ArgumentNotNullOrEmpty(this.ModuleId, nameof(this.ModuleId));
            Validate.ArgumentNotNullOrEmpty(this.DeviceId, nameof(this.DeviceId));
            Validate.ArgumentNotNullOrEmpty(this.IotHubHostName, nameof(this.IotHubHostName));
            Validate.ArgumentNotNullOrEmpty(this.moduleGenerationId, nameof(this.moduleGenerationId));
            Validate.ArgumentNotNullOrEmpty(this.edgeGatewayHostName, nameof(this.edgeGatewayHostName));
            Validate.ArgumentNotNullOrEmpty(this.workloadApiVersion, nameof(this.workloadApiVersion));
            Validate.ArgumentNotNullOrEmpty(workloadUriString, nameof(workloadUriString));

            this.IotHubName = this.IotHubHostName.Split('.').FirstOrDefault();

            var workloadUri = new Uri(workloadUriString);

            string baseUrlForRequests;
            if (workloadUri.Scheme.Equals(SecurityDaemonClient.UnixScheme, StringComparison.OrdinalIgnoreCase))
            {
                baseUrlForRequests = $"http://{workloadUri.Segments.Last()}";
                this.httpClient = new HttpClient(new HttpUdsMessageHandler(workloadUri));
            }
            else if (workloadUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                workloadUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                baseUrlForRequests = workloadUriString;
                this.httpClient = new HttpClient();
            }
            else
            {
                throw new InvalidOperationException($"Unknown workloadUri scheme specified. {workloadUri}");
            }

            baseUrlForRequests = baseUrlForRequests.TrimEnd();
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string encodedApiVersion = UrlEncoder.Default.Encode(this.workloadApiVersion);
            string encodedModuleId = UrlEncoder.Default.Encode(this.ModuleId);
            string encodedModuleGenerationId = UrlEncoder.Default.Encode(this.moduleGenerationId);

            this.getTrustBundleUri = new Uri($"{baseUrlForRequests}/trust-bundle?api-version={encodedApiVersion}");
            this.postIdentityCertificateRequestUri = new Uri($"{baseUrlForRequests}/modules/{encodedModuleId}/certificate/identity?api-version={encodedApiVersion}");
            this.postServerCertificateRequestUri = new Uri($"{baseUrlForRequests}/modules/{encodedModuleId}/genid/{encodedModuleGenerationId}/certificate/server?api-version={encodedApiVersion}");
            this.postSignRequestUri = new Uri($"{baseUrlForRequests}/modules/{encodedModuleId}/genid/{encodedModuleGenerationId}/sign?api-version={encodedApiVersion}");
            this.postEncryptRequestUri = new Uri($"{baseUrlForRequests}/modules/{encodedModuleId}/genid/{encodedModuleGenerationId}/encrypt?api-version={encodedApiVersion}");
            this.postDecryptRequestUri = new Uri($"{baseUrlForRequests}/modules/{encodedModuleId}/genid/{encodedModuleGenerationId}/decrypt?api-version={encodedApiVersion}");


            var settings = new
            {
                this.ModuleId,
                this.DeviceId,
                IotHubHostName = this.IotHubHostName,
                ModuleGenerationId = this.moduleGenerationId,
                GatewayHostName = this.edgeGatewayHostName,
                WorkloadUri = workloadUriString,
                WorkloadApiVersion = this.workloadApiVersion,
            };
            this.asString = $"{nameof(SecurityDaemonClient)}{JsonConvert.SerializeObject(settings, Formatting.None, this.jsonSettings)}";
        }

        public string IotHubHostName { get; }

        public string IotHubName { get; }

        public string DeviceId { get; }

        public string ModuleId { get; }

        public void Dispose() => this.httpClient.Dispose();

        public override string ToString() => this.asString;

        public Task<(X509Certificate2 serverCert, X509Certificate2[] certChain)> GetServerCertificateAsync(CancellationToken token = default)
        {
            return this.GetServerCertificateAsync(TimeSpan.FromDays(SecurityDaemonClient.DefaultServerCertificateValidityInDays), token);
        }

        public async Task<(X509Certificate2 serverCert, X509Certificate2[] certChain)> GetServerCertificateAsync(TimeSpan validity, CancellationToken token = default)
        {
            var request = new ServerCertificateRequest
            {
                CommonName = this.edgeGatewayHostName,
                Expiration = DateTime.UtcNow.Add(validity),
            };

            string requestString = JsonConvert.SerializeObject(request, Formatting.None, this.jsonSettings);
            using (var content = new StringContent(requestString, Encoding.UTF8, "application/json"))
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, this.postServerCertificateRequestUri) { Content = content })
            using (HttpResponseMessage httpResponse = await this.httpClient.SendAsync(httpRequest, token))
            {
                string responsePayload = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.StatusCode == HttpStatusCode.Created)
                {
                    CertificateResponse cr = JsonConvert.DeserializeObject<CertificateResponse>(responsePayload, this.jsonSettings);
                    return this.CreateX509Certificates(cr);
                }

                throw new InvalidOperationException($"Failed to retrieve server certificate from IoTEdge security daemon. StatusCode={httpResponse.StatusCode} ReasonPhrase='{httpResponse.ReasonPhrase}' ResponsePayload='{responsePayload}' Request={requestString} This={this}");
            }
        }

        public Task<(X509Certificate2 identityCert, X509Certificate2[] certChain)> GetIdentityCertificateAsync(CancellationToken token = default)
        {
            return this.GetIdentityCertificateAsync(TimeSpan.FromDays(SecurityDaemonClient.DefaultIdentityCertificateValidityInDays), token);
        }

        public async Task<(X509Certificate2 identityCert, X509Certificate2[] certChain)> GetIdentityCertificateAsync(TimeSpan validity, CancellationToken token = default)
        {
            var request = new IdentityCertificateRequest
            {
                Expiration = DateTime.UtcNow.Add(validity),
            };

            string requestString = JsonConvert.SerializeObject(request, Formatting.None, this.jsonSettings);
            using (var content = new StringContent(requestString, Encoding.UTF8, "application/json"))
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, this.postIdentityCertificateRequestUri) { Content = content })
            using (HttpResponseMessage httpResponse = await this.httpClient.SendAsync(httpRequest, token))
            {
                string responsePayload = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    CertificateResponse cr = JsonConvert.DeserializeObject<CertificateResponse>(responsePayload, this.jsonSettings);
                    return this.CreateX509Certificates(cr);
                }

                throw new InvalidOperationException($"Failed to retrieve identity certificate from IoTEdge security daemon. StatusCode={httpResponse.StatusCode} ReasonPhrase='{httpResponse.ReasonPhrase}' ResponsePayload='{responsePayload}' Request={requestString} This={this}");
            }
        }

        public async Task<X509Certificate2[]> GetTrustBundleAsync(CancellationToken token = default)
        {
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Get, this.getTrustBundleUri))
            using (HttpResponseMessage httpResponse = await this.httpClient.SendAsync(httpRequest, token))
            {
                string responsePayload = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    TrustBundleResponse trustBundleResponse = JsonConvert.DeserializeObject<TrustBundleResponse>(responsePayload, this.jsonSettings);
                    Validate.ArgumentNotNullOrEmpty(trustBundleResponse.Certificate, nameof(trustBundleResponse.Certificate));

                    string[] rawCerts = ParseCertificateResponse(trustBundleResponse.Certificate);
                    if (rawCerts.FirstOrDefault() == null)
                    {
                        throw new InvalidOperationException($"Failed to retrieve the certificate trust bundle from IoTEdge security daemon. StatusCode={httpResponse.StatusCode} ReasonPhrase='{httpResponse.ReasonPhrase}' Reason='Security daemon returned an empty response' This={this}");
                    }

                    return ConvertToX509(rawCerts);
                }

                throw new InvalidOperationException($"Failed to retrieve the certificate trust bundle from IoTEdge security daemon. StatusCode={httpResponse.StatusCode} ReasonPhrase='{httpResponse.ReasonPhrase}' ResponsePayload='{responsePayload}' This={this}");
            }
        }

        public async Task<SignResponse> SignAsync(byte[] data, CancellationToken token = default)
        {
            var request = new SignRequest
            {
                Algo = SignRequestAlgo.HMACSHA256,
                Data = data,
                KeyId = "primary"
            };

            string requestString = JsonConvert.SerializeObject(request, Formatting.None, this.jsonSettings);
            using (var content = new StringContent(requestString, Encoding.UTF8, "application/json"))
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, this.postSignRequestUri) { Content = content })
            using (HttpResponseMessage httpResponse = await this.httpClient.SendAsync(httpRequest, token))
            {
                string responsePayload = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    SignResponse signResponse = JsonConvert.DeserializeObject<SignResponse>(responsePayload, this.jsonSettings);
                    return signResponse;
                }

                throw new InvalidOperationException($"Failed to execute sign request from IoTEdge security daemon. StatusCode={httpResponse.StatusCode} ReasonPhrase='{httpResponse.ReasonPhrase}' ResponsePayload='{responsePayload}' Request={requestString} This={this}");
            }
        }

        public async Task<string> EncryptAsync(string plaintext, string initializationVector, CancellationToken token = default)
        {
            var request = new EncryptRequest
            {
                InitializationVector = Convert.ToBase64String(Encoding.UTF8.GetBytes(initializationVector)),
                Plaintext = Convert.ToBase64String(Encoding.UTF8.GetBytes(plaintext))
            };

            string requestString = JsonConvert.SerializeObject(request, Formatting.None, this.jsonSettings);
            using (var content = new StringContent(requestString, Encoding.UTF8, "application/json"))
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, this.postEncryptRequestUri) { Content = content })
            using (HttpResponseMessage httpResponse = await this.httpClient.SendAsync(httpRequest, token))
            {
                string responsePayload = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    EncryptResponse encryptResponse = JsonConvert.DeserializeObject<EncryptResponse>(responsePayload, this.jsonSettings);
                    return encryptResponse.Ciphertext;
                }

                throw new InvalidOperationException($"Failed to execute sign request from IoTEdge security daemon. StatusCode={httpResponse.StatusCode} ReasonPhrase='{httpResponse.ReasonPhrase}' ResponsePayload='{responsePayload}' Request={requestString} This={this}");
            }
        }

        public async Task<string> DecryptAsync(string ciphertext, string initializationVector, CancellationToken token = default)
        {
            var request = new DecryptRequest
            {
                InitializationVector = Convert.ToBase64String(Encoding.UTF8.GetBytes(initializationVector)),
                Ciphertext = ciphertext
            };

            string requestString = JsonConvert.SerializeObject(request, Formatting.None, this.jsonSettings);
            using (var content = new StringContent(requestString, Encoding.UTF8, "application/json"))
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, this.postDecryptRequestUri) { Content = content })
            using (HttpResponseMessage httpResponse = await this.httpClient.SendAsync(httpRequest, token))
            {
                string responsePayload = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    DecryptResponse decryptResponse = JsonConvert.DeserializeObject<DecryptResponse>(responsePayload, this.jsonSettings);
                    
                    return Encoding.UTF8.GetString(Convert.FromBase64String(decryptResponse.Plaintext));
                }

                throw new InvalidOperationException($"Failed to execute sign request from IoTEdge security daemon. StatusCode={httpResponse.StatusCode} ReasonPhrase='{httpResponse.ReasonPhrase}' ResponsePayload='{responsePayload}' Request={requestString} This={this}");
            }
        }

        public async Task<string> GetModuleToken(int expiryInSeconds = 3600)
        { 
            TimeSpan fromEpochStart = DateTime.UtcNow - new DateTime(1970, 1, 1);
            string expiry = Convert.ToString((int)fromEpochStart.TotalSeconds + expiryInSeconds);

            string resourceUri = $"{this.IotHubHostName}/devices/{this.DeviceId}/modules/{this.ModuleId}";

            string stringToSign = WebUtility.UrlEncode(resourceUri) + "\n" + expiry;

            var signResponse = await this.SignAsync(Encoding.UTF8.GetBytes(stringToSign));

            var signature = Convert.ToBase64String(signResponse.Digest);

            string token = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}", 
                WebUtility.UrlEncode(resourceUri), WebUtility.UrlEncode(signature), expiry);          

            return token;
        }

        private static X509Certificate2[] ConvertToX509(IEnumerable<string> rawCerts) => rawCerts.Select(c => new X509Certificate2(Encoding.UTF8.GetBytes(c))).ToArray();

        private static string[] ParseCertificateResponse(string certificateChain, [CallerMemberName] string callerMemberName = default)
        {
            if (string.IsNullOrEmpty(certificateChain))
            {
                throw new InvalidOperationException($"Trusted certificates can not be null or empty for {callerMemberName}.");
            }

            // Extract each certificate's string. The final string from the split will either be empty
            // or a non-certificate entry, so it is dropped.
            string delimiter = "-----END CERTIFICATE-----";
            string[] rawCerts = certificateChain.Split(new[] { delimiter }, StringSplitOptions.None);
            return rawCerts.Take(count: rawCerts.Length - 1).Select(c => $"{c}{delimiter}").ToArray();
        }

        private (X509Certificate2 primaryCert, X509Certificate2[] certChain) CreateX509Certificates(CertificateResponse cr, [CallerMemberName] string callerMemberName = default)
        {
            Validate.ArgumentNotNullOrEmpty(cr.Certificate, nameof(cr.Certificate));
            Validate.ArgumentNotNull(cr.Expiration, nameof(cr.Expiration));
            Validate.ArgumentNotNull(cr.PrivateKey, nameof(cr.PrivateKey));
            Validate.ArgumentNotNull(cr.PrivateKey.Type, nameof(cr.PrivateKey.Type));
            Validate.ArgumentNotNull(cr.PrivateKey.Bytes, nameof(cr.PrivateKey.Bytes));

            string[] rawCerts = ParseCertificateResponse(cr.Certificate);
            if (rawCerts.Length == 0 ||
                string.IsNullOrWhiteSpace(rawCerts[0]))
            {
                throw new InvalidOperationException($"Failed to retrieve certificate from IoTEdge Security daemon for {callerMemberName}. Reason: Security daemon returned an empty response.");
            }

            string primaryCert = rawCerts[0];
            X509Certificate2[] certChain = ConvertToX509(rawCerts.Skip(1));

            RsaPrivateCrtKeyParameters keyParams = null;

            var chainCertEntries = new List<X509CertificateEntry>();
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            // note: the seperator between the certificate and private key is added for safety to delineate the cert and key boundary
            using (var sr = new StringReader(primaryCert + "\r\n" + cr.PrivateKey.Bytes))
            {
                var pemReader = new PemReader(sr);
                object certObject;
                while ((certObject = pemReader.ReadObject()) != null)
                {
                    if (certObject is Org.BouncyCastle.X509.X509Certificate x509Cert)
                    {
                        chainCertEntries.Add(new X509CertificateEntry(x509Cert));
                    }

                    // when processing certificates generated via openssl certObject type is of AsymmetricCipherKeyPair
                    if (certObject is AsymmetricCipherKeyPair ackp)
                    {
                        certObject = ackp.Private;
                    }

                    if (certObject is RsaPrivateCrtKeyParameters rpckp)
                    {
                        keyParams = rpckp;
                    }
                }
            }

            if (keyParams == null)
            {
                throw new InvalidOperationException($"Private key was not found for {callerMemberName}");
            }

            store.SetKeyEntry(this.ModuleId, new AsymmetricKeyEntry(keyParams), chainCertEntries.ToArray());
            using (var ms = new MemoryStream())
            {
                store.Save(ms, Array.Empty<char>(), new SecureRandom());
                var x509PrimaryCert = new X509Certificate2(ms.ToArray());
                return (x509PrimaryCert, certChain);
            }
        }
    }
}
