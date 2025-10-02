using System.Collections.Generic;
using System.Threading.Tasks;

namespace BirokratNext.api_clientv2
{
    public interface IDocumentCalls : IMyLoggable
    {
        Task<string> CreateSimpleJson(string path, string eslog);
        Task<string> UpdateSimpleJson(string path, string eslog, string sifra);
        Task<string> GetSimplejson(string path, string sifra);
        Task<string> Delete(string path, string sifra);
        Task<string> Fiscalize(string path, string sifra);
        Task<string> GetEslog(string path, string sifra);
        Task<string> CreateEslog(string path, string eslog);
        Task<string> GetPdf(string path, string sifra);
        Task<string> Navigation();
        Task<List<PLParameterResponseRecord>> Update(string path, string sifra, Dictionary<string, object> pars = null);
        Task<List<PLParameterResponseRecord>> UpdateParameters(string path, string sifra, Dictionary<string, object> pars = null);


        Task<List<Dictionary<string, object>>> GetSpecification(string path, string sifra);
        Task<List<PLParameterResponseRecord>> UpdateSpecificationParameters(string path, string sifra, int row, Dictionary<string, object> pars = null);
        Task<string> UpdateSpecification(string path, string sifra, int row, Dictionary<string, object> pars = null);
        Task<string> DeleteSpecification(string path, string sifra, int row);
        Task<List<PLParameterResponseRecord>> AddSpecificationParameters(string path, string sifra, Dictionary<string, object> pars = null);
        Task<string> AddSpecification(string path, string sifra, Dictionary<string, object> pars = null);
    }
}
