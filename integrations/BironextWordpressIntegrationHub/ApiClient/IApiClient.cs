using BirokratNext.Models;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;

namespace BirokratNext
{
    public interface IApiClient
    {
        public Task Start();
        public Task<object> Test();
        public Task<List<Parameter>> KumulativaParametri(string key, List<Parameter> parameters = null);
        public Task<List<Parameter>> SifrantParametri(string key, List<Parameter> parameters = null);
        public Task<DataSet> KumulativaPodatki(string key, List<Parameter> parameters = null);
        public Task<string> KumulativaPodatkiJson(string key, List<Parameter> parameters = null);
        public Task<DataSet> SifrantPodatki(string key, List<Parameter> parameters = null);
        public Task<string> SifrantPodatkiJson(string key, List<Parameter> parameters = null);
        public void Dispose();
    }
}
