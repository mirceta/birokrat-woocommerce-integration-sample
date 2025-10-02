using System.Collections.Generic;
using System.Threading.Tasks;

namespace BirokratNext.api_clientv2
{
    public interface ICumulativeCalls : IMyLoggable
    {
        Task<string> Navigation();
        Task<List<PLParameterResponseRecord>> Parametri(string path, Dictionary<string, object> parameters = null);
        Task<List<Dictionary<string, object>>> Podatki(string path, Dictionary<string, object> parameters = null, bool excel = false);
    }
}