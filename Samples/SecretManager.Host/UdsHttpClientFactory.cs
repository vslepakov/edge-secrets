using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;

namespace SecretManager.Host
{
    internal class UdsHttpClientFactory : IUdsHttpClientFactory
    {
        private readonly CancellationToken _cancellationToken;

        public UdsHttpClientFactory(CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
        }

        public HttpClient CreateHttpClientForSocket(string socketAddress)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                throw new InvalidOperationException("Cannot create UdsHttpClients with canceled tokens");
            }

            return new HttpClient(new SocketsHttpHandler
            {
                ConnectCallback = async (context, token) =>
                {
                    var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                    var endpoint = new UnixDomainSocketEndPoint(socketAddress);
                    await socket.ConnectAsync(endpoint, _cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
            });
        }
    }
}
