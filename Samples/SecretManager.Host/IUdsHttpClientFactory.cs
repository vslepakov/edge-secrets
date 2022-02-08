using System.Net.Http;

namespace SecretManager.Host
{
    internal interface IUdsHttpClientFactory
    {
        HttpClient CreateHttpClientForSocket(string socketAddress);
    }
}
