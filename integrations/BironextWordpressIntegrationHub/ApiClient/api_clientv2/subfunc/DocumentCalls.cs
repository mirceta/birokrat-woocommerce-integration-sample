using BirokratNext.Utils;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
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

        // get - eslog
        public async Task<string> GetEslog(string path, string sifra)
        {
            path = Path.Combine("v2", path, "geteslog", sifra);

            var tmp = HttpClientFactory.Create();
            tmp.Timeout = new TimeSpan(0, 3, 0);
            string some = (string)await HttpGet(path, tmp);
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

        public Task<string> GetSimplejson(string path, string sifra) {
            throw new NotImplementedException();
        }

        public Task<string> CreateEslog(string path, string eslog) {
            throw new NotImplementedException();
        }

        public Task<List<Dictionary<string, object>>> GetSpecification(string path, string sifra)
        {
            throw new NotImplementedException();
        }

        public Task<List<PLParameterResponseRecord>> UpdateSpecificationParameters(string path, string sifra, int row, Dictionary<string, object> pars = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateSpecification(string path, string sifra, int row, Dictionary<string, object> pars = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> DeleteSpecification(string path, string sifra, int row)
        {
            throw new NotImplementedException();
        }

        public Task<List<PLParameterResponseRecord>> AddSpecificationParameters(string path, string sifra, Dictionary<string, object> pars = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> AddSpecification(string path, string sifra, Dictionary<string, object> pars = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateSimpleJson(string path, string eslog, string sifra)
        {
            throw new NotImplementedException();
        }
    }
}
