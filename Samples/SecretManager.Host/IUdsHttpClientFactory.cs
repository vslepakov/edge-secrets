using System.Net.Http;
using System.Threading;

namespace SecretManager.Host
{
    internal interface IUdsHttpClientFactory
    {
        HttpClient CreateHttpClientForSocket(string socketAddress, CancellationToken cancellationToken = default);
    }
}
