using BirokratNext.Models;
using BirokratNext.Utils;
using Newtonsoft.Json;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BirokratNext.api_clientv2
{
    public class CumulativeCalls : FunctionalityCall, ICumulativeCalls
    {
        public CumulativeCalls(IHttpClientFactory factory, HttpClient client, IMyLogger logger) : base(client, factory, logger)
        {
        }

        public async Task<string> Navigation()
        {
            var response = await HttpClient.GetAsync("v2/kumulativa/navigacija");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<PLParameterResponseRecord>> Parametri(string path, Dictionary<string, object> parameters = null)
        {
            path = Path.Combine("v2", path, "parameters");
            string content = "";
            if (parameters != null)
                content = Serializer.ToJson(parameters);
            string some = (string)await HttpPost(path, content);
            return Serializer.FromJson<List<PLParameterResponseRecord>>(some);
        }

        public async Task<List<Dictionary<string, object>>> Podatki(string path, Dictionary<string, object> parameters = null, bool excel = false)
        {
            path = Path.Combine("v2", path, "data") + $"?excel={excel}";
            string content = "";
            if (parameters != null)
                content = Serializer.ToJson(parameters);
            string result = (string)await HttpPost(path, content);
            var dic = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(result);
            string some = dic.Keys.ToList()[0];
            return dic[some];
        }
    }
}
