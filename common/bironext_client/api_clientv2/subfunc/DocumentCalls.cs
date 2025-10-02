using BirokratNext.Utils;
using Newtonsoft.Json;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BirokratNext.api_clientv2
{
    public class DocumentCalls : FunctionalityCall, IDocumentCalls
    {
        public DocumentCalls(IHttpClientFactory factory, HttpClient client, IMyLogger logger) : base(client, factory, logger)
        {
        }


        public async Task<string> Navigation()
        {
            var response = await HttpClient.GetAsync("v2/dokument/navigacija");
            return await response.Content.ReadAsStringAsync();
        }

        // update
        public async Task<List<PLParameterResponseRecord>> UpdateParameters(string path, string sifra, Dictionary<string, object> pars = null)
        {
            path = Path.Combine("v2", path, "update/parameters/", sifra);
            string content = "";
            if (pars != null)
                content = Serializer.ToJson(pars);
            string some = (string)await HttpPost(path, content);
            return Serializer.FromJson<List<PLParameterResponseRecord>>(some);
        }

        public async Task<List<PLParameterResponseRecord>> Update(string path, string sifra, Dictionary<string, object> pars = null)
        {
            path = Path.Combine("v2", path, "update/", sifra);
            string content = "";
            if (pars != null)
                content = Serializer.ToJson(pars);
            string some = (string)await HttpPost(path, content);
            return Serializer.FromJson<List<PLParameterResponseRecord>>(some);
        }

        // create - eslog
        public async Task<string> CreateSimpleJson(string path, string eslog)
        {
            path = Path.Combine("v2", path, "create", "simplejson");
            string some = (string)await HttpPost(path, eslog);
            if (some.Contains("error|"))
            {
                throw new BironextApiCallException(some.Substring(some.IndexOf("error|")), null);
            }
            return some;
        }

        // create - eslog
        public async Task<string> CreateEslog(string path, string eslog)
        {
            path = Path.Combine("v2", path, "create", "eslog");
            string some = (string)await HttpPost(path, eslog);
            return some;
        }

        // update - eslog
        public async Task<string> UpdateSimpleJson(string path, string eslog, string sifra)
        {
            path = Path.Combine("v2", path, "update", "simplejson", sifra);
            string some = (string)await HttpPost(path, eslog);
            if (some.Contains("error|"))
            {
                throw new BironextApiCallException(some.Substring(some.IndexOf("error|")), null);
            }
            return some;
        }

        // get - eslog
        public async Task<string> GetEslog(string path, string sifra)
        {
            path = Path.Combine("v2", path, "geteslog", sifra);

            var tmp = HttpClientFactory.Create();
            tmp.Timeout = new TimeSpan(0, 3, 0);
            string some = (string)await HttpGet(path, tmp);
            return some;

        }

        // get - simplejson
        public async Task<string> GetSimplejson(string path, string sifra)
        {
            path = Path.Combine("v2", path, "getsimplejson", sifra);
            string some = (string)await HttpGet(path);
            return some;
        }

        // get - pdf
        public async Task<string> GetPdf(string path, string sifra)
        {
            path = Path.Combine("v2", path, "getpdf", sifra);
            string some = (string)await HttpGet(path);
            return some;
        }

        // fiskaliziraj
        public async Task<string> Fiscalize(string path, string sifra)
        {
            path = Path.Combine("v2", path, "fiscalize", sifra);
            string some = (string)await HttpGet(path);
            return some;
        }

        // delete
        public async Task<string> Delete(string path, string sifra)
        {
            path = Path.Combine("v2", path, "delete", sifra);
            string some = (string)await HttpDelete(path);
            return some;
        }


        #region [specification]

        public async Task<List<Dictionary<string, object>>> GetSpecification(string path, string sifra)
        {
            path = Path.Combine("v2", path, "specification", sifra);
            string response = (string)await HttpGet(path);
            var result = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response);
            return result;
        }

        public async Task<List<PLParameterResponseRecord>> UpdateSpecificationParameters(string path, string invoicenum, int row, Dictionary<string, object> pars = null)
        {
            path = Path.Combine("v2", path, "specification", invoicenum, "update", "parameters", row.ToString());
            string content = pars != null ? Serializer.ToJson(pars) : "";
            string response = (string)await HttpPost(path, content);
            var result = JsonConvert.DeserializeObject<List<PLParameterResponseRecord>>(response);
            return result;
        }

        public async Task<string> UpdateSpecification(string path, string invoicenum, int row, Dictionary<string, object> pars = null)
        {
            path = Path.Combine("v2", path, "specification", invoicenum, "update", row.ToString());
            string content = pars != null ? Serializer.ToJson(pars) : "";
            string response = (string)await HttpPost(path, content);
            return response;
        }

        public async Task<string> DeleteSpecification(string path, string invoicenum, int row)
        {
            path = Path.Combine("v2", path, "specification", invoicenum, "delete", row.ToString());
            string response = (string)await HttpDelete(path);
            return response;
        }

        public async Task<List<PLParameterResponseRecord>> AddSpecificationParameters(string path, string invoicenum, Dictionary<string, object> pars = null)
        {
            path = Path.Combine("v2", path, "specification", invoicenum, "create", "parameters");
            string content = pars != null ? Serializer.ToJson(pars) : "";
            string response = (string)await HttpPost(path, content);
            var result = JsonConvert.DeserializeObject<List<PLParameterResponseRecord>>(response);
            return result;
        }

        public async Task<string> AddSpecification(string path, string invoicenum, Dictionary<string, object> pars = null)
        {
            path = Path.Combine("v2", path, "specification", invoicenum, "create");
            string content = pars != null ? Serializer.ToJson(pars) : "";
            string response = (string)await HttpPost(path, content);
            return response;
        }
        #endregion

    }
}
