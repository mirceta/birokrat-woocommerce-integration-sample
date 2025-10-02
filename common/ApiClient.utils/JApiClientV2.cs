using BirokratNext.api_clientv2;
using si.birokrat.next.common.logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace BirokratNext
{
    public interface IApiClientV2
    {
        string ApiKey { get; set; }
        ICumulativeCalls cumulative { get; }
        IDocumentCalls document { get; }
        HttpClient HttpClient { get; }
        IHttpClientFactory httpClientFactory { get; }
        ISifrantCalls sifrant { get; }
        IUtilitiesCalls utilities { get; }

        void Dispose();
        Task<object> Logout();
        void SetLogger(IMyLogger logger);
        Task Start();
        Task<object> Test();
    }
}