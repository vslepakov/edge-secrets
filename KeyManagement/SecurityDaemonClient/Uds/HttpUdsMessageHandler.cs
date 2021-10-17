// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.EventGridEdge.IotEdge
{
    /// <summary>
    /// Unix domain message handler.
    /// </summary>
    internal class HttpUdsMessageHandler : HttpMessageHandler
    {
        private readonly Uri providerUri;

        public HttpUdsMessageHandler(Uri providerUri)
        {
            Validate.ArgumentNotNull(providerUri, nameof(providerUri));
            this.providerUri = providerUri;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Validate.ArgumentNotNull(request, nameof(request));

            using (Socket socket = await this.GetConnectedSocketAsync())
            {
                using (var stream = new HttpBufferedStream(new NetworkStream(socket, true)))
                {
                    var serializer = new HttpRequestResponseSerializer();
                    byte[] requestBytes = serializer.SerializeRequest(request);

                    await stream.WriteAsync(requestBytes, 0, requestBytes.Length, cancellationToken);
                    if (request.Content != null)
                    {
                        await request.Content.CopyToAsync(stream);
                    }

                    return await serializer.DeserializeResponseAsync(stream, cancellationToken);
                }
            }
        }

        private async Task<Socket> GetConnectedSocketAsync()
        {
            var endpoint = new UnixDomainSocketEndPoint(this.providerUri.LocalPath);
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            await socket.ConnectAsync(endpoint);
            return socket;
        }
    }
}
