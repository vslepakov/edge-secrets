using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;

namespace SecretManager.Host
{
    internal class UdsHttpClientFactory : IUdsHttpClientFactory
    {
        public HttpClient CreateHttpClientForSocket(string socketAddress, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new InvalidOperationException("Cannot create UdsHttpClients with canceled tokens");
            }

            return new HttpClient(new SocketsHttpHandler
            {
                ConnectCallback = async (context, token) =>
                {
                    var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                    var endpoint = new UnixDomainSocketEndPoint(socketAddress);
                    await socket.ConnectAsync(endpoint, cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
            });
        }
    }
}
