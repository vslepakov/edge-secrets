using System.Net.Http;
using System.Net.Sockets;
using System.Threading;

namespace KeyManagementService
{
    internal class HttpClientHelper
    {
        public static HttpClient GetUnixDomainSocketHttpClient(string socketPath, CancellationToken ct)
        {
            return new HttpClient(new SocketsHttpHandler
            {
                ConnectCallback = async (context, token) =>
                {
                    var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                    var endpoint = new UnixDomainSocketEndPoint(socketPath);
                    await socket.ConnectAsync(endpoint, ct);
                    return new NetworkStream(socket, ownsSocket: true);
                }
            });
        }
    }
}
