using System.Collections.Generic;
using System.Threading.Tasks;

namespace BirokratNext.api_clientv2
{
    public interface ISifrantCalls : IMyLoggable
    {
        Task<object> Create(string path, Dictionary<string, object> parameters = null);
        Task<List<PLParameterResponseRecord>> CreateParameters(string path, Dictionary<string, object> parameters = null);
        Task<string> Delete(string path, string id);
        Task<string> Navigation();
        Task<bool> Pagelen(int len);
        Task<List<PLParameterResponseRecord>> Parameters(string path, Dictionary<string, object> pars = null);
        Task<List<Dictionary<string, object>>> Podatki(string path, string query = null, Dictionary<string, object> pars = null, string page = null);
        Task<object> Update(string path, Dictionary<string, object> parameters = null);
        Task<List<PLParameterResponseRecord>> UpdateParameters(string path, string id);
    }
}