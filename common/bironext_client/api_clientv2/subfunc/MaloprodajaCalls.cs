using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BirokratNext.api_clientv2.subfunc {
    public class MaloprodajaCalls : FunctionalityCall {
        public MaloprodajaCalls(IHttpClientFactory factory, HttpClient client, IMyLogger logger) : base(client, factory, logger)
        { }

        public async Task<string> CreateSimpleJson(string path, string json) {
            path = Path.Combine("v2", path, "create", "simplejson");
            string some = (string)await HttpPost(path, json);
            return some;
        }

        public async Task<string> Get(string path, string sifra) {
            path = Path.Combine("v2", path, "get", sifra);
            string some = (string)await HttpGet(path);
            return some;
        }
    }
}
