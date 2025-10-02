using System.Net.Http;

namespace BirokratNext
{
    public interface IHttpClientFactory
    {
        HttpClient Create();
    }
}