using BirokratNext.Models;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;

namespace BirokratNext
{
    public interface IApiClient
    {
        Task Start();
        Task<object> Test();
        Task<List<Parameter>> KumulativaParametri(string key, List<Parameter> parameters = null);
        Task<List<Parameter>> SifrantParametri(string key, List<Parameter> parameters = null);
        Task<DataSet> KumulativaPodatki(string key, List<Parameter> parameters = null);
        Task<string> KumulativaPodatkiJson(string key, List<Parameter> parameters = null);
        Task<DataSet> SifrantPodatki(string key, List<Parameter> parameters = null);
        Task<string> SifrantPodatkiJson(string key, List<Parameter> parameters = null);
        void Dispose();
    }
}
