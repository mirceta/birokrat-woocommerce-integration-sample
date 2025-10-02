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
using Xunit.Sdk;

namespace BirokratNext.api_clientv2 {
    public class SifrantCalls : FunctionalityCall, ISifrantCalls
    {
        public SifrantCalls(IHttpClientFactory factory, HttpClient client, IMyLogger logger) : base(client, factory, logger)
        {
        }

        public async Task<string> Navigation()
        {
            var response = await HttpClient.GetAsync("v2/sifrant/navigacija");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<bool> Pagelen(int len)
        {
            var response = await HttpClient.GetAsync($"v2/sifrant/pagelen?val={len}");
            string res = await response.Content.ReadAsStringAsync();
            return true;
        }

        public async Task<List<PLParameterResponseRecord>> Parameters(string path, Dictionary<string, object> pars = null)
        {
            path = Path.Combine("v2", path, "parameters");
            string content = "";
            if (pars != null)
                content = Serializer.ToJson(pars);
            string result = (string)await HttpPost(path, content);
            return Serializer.FromJson<List<PLParameterResponseRecord>>(result);
        }

        public async Task<List<Dictionary<string, object>>> Podatki(string path, string query = null, Dictionary<string, object> pars = null, string page = null)
        {
            try
            {
                path = Path.Combine("v2", path);
                if (query != null || page != null)
                    path = Path.Combine(path, "?");
                if (query != null)
                    path += $"query={query}";
                if (page != null)
                {
                    if (path.Contains($"query={query}"))
                    {
                        path = path + "&";
                    }
                    path += $"page={page}";
                }
                string content = "";
                if (pars != null)
                    content = Serializer.ToJson(pars);
                string result = (string)await HttpPost(path, content);

                try
                {
                    var dic = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(result);
                    string some = dic.Keys.ToList()[0];
                    return dic[some];
                }
                catch (Exception ex)
                {
                    throw new Exception(result + ex.Message, ex);
                }
            }
            catch (Exception ex)
            {
                throw new BironextApiCallException("", ex);
            }

        }

        public async Task<List<PLParameterResponseRecord>> CreateParameters(string path, Dictionary<string, object> parameters = null)
        {
            path = Path.Combine("v2", path, "create/parameters");
            string some = (string)await HttpPost(path);
            return Serializer.FromJson<List<PLParameterResponseRecord>>(some);
        }

        public async Task<object> Create(string path, Dictionary<string, object> parameters = null)
        {
            path = Path.Combine("v2", path, "create");
            var content = Serializer.ToJson(parameters);
            return await HttpPost(path, content);
        }

        public async Task<List<PLParameterResponseRecord>> UpdateParameters(string path, string id)
        {
            path = Path.Combine("v2", path, "update", "parameters", id.Replace("/", "%2F"));
            string some = (string)await HttpPost(path);
            if (some.StartsWith("Error") && some.Contains("Ni najden"))
            {
                throw new SifrantRecordNotFoundException(some, null);
            }
            try
            {
                return Serializer.FromJson<List<PLParameterResponseRecord>>(some);
            }
            catch (Exception ex)
            {
                throw new BironextApiCallException(ex.Message, ex.InnerException);
            }
        }

        public async Task<object> Update(string path, Dictionary<string, object> parameters = null)
        {
            path = Path.Combine("v2", path, "update");
            var content = Serializer.ToJson(parameters);
            return await HttpPost(path, content);
        }

        public async Task<string> Delete(string path, string id)
        {
            path = Path.Combine("v2", path, id);
            string retval = (string)await HttpDelete(path);
            return retval;
        }

    }

    public class SifrantRecordNotFoundException : BironextApiCallException {
        public SifrantRecordNotFoundException(string message, Exception inner) : base(message, inner) { }
    }

    public class BironextApiCallException : Exception {
        public BironextApiCallException(string message, Exception inner) : base(message, inner) { }
    }
}
