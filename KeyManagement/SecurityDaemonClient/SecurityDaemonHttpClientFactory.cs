// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.EventGridEdge.IotEdge
{
    [Obsolete("This is a pubternal API that's being made public as a stop-gap measure. It will be removed from the Event Grid SDK nuget package as soon IoT Edge SDK ships with a built-in a security daemon client.")]
    public class SecurityDaemonHttpClientFactory : IHttpClientFactory
    {
        private readonly Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> callback;

        public SecurityDaemonHttpClientFactory(X509Certificate2 identityCertificate)
            : this(identityCertificate, ServiceCertificateValidationCallback)
        {
        }

        public SecurityDaemonHttpClientFactory(X509Certificate2 identityCertificate, Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> serverCertificateCallback)
        {
            this.IdentityCertificate = identityCertificate;
            this.callback = serverCertificateCallback;
        }

        public X509Certificate2 IdentityCertificate { get; }

        public static async Task<SecurityDaemonHttpClientFactory> CreateAsync(CancellationToken token = default)
        {
            using (var iotEdgeClient = new SecurityDaemonClient())
            {
                (X509Certificate2 identityCertificate, _) = await iotEdgeClient.GetIdentityCertificateAsync(token);
                return new SecurityDaemonHttpClientFactory(identityCertificate);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000: DisposeObjectsBeforeLosingScope", Justification = "The HttpClient owns the lifetime of the handler")]
        public HttpClient CreateClient(string name)
        {
            var httpClientHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = this.callback };
            httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
            httpClientHandler.ClientCertificates.Add(this.IdentityCertificate);
            return new HttpClient(httpClientHandler, disposeHandler: true);
        }

        private static bool ServiceCertificateValidationCallback(HttpRequestMessage request, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}
