using ApiClient.utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BirokratNext
{
    public class ApiClientV3Document {

        IApiClientV2 client;

        public ApiClientV3Document(IApiClientV2 client) {
            this.client = client;
        }

        // update
        public async Task<List<PLParameterResponseRecord>> UpdateParameters(BirokratDocumentType path, string sifra, Dictionary<string, object> pars = null) {
            return await client.document.UpdateParameters(BironextApiPathHelper.GetVnosByType(path), sifra, pars);
        }

        public async Task<List<PLParameterResponseRecord>> Update(BirokratDocumentType path, string sifra, Dictionary<string, object> pars = null) {
            return await client.document.Update(BironextApiPathHelper.GetVnosByType(path), sifra, pars);
        }

        // create - eslog
        public async Task<string> CreateSimplejson(BirokratDocumentType path, string eslog) {
            return await client.document.CreateSimpleJson(BironextApiPathHelper.GetVnosByType(path), eslog);
        }

        // get - eslog
        public async Task<string> GetEslog(BirokratDocumentType path, string sifra) {
            return await client.document.GetEslog(BironextApiPathHelper.GetVnosByType(path), sifra);
        }

        // get - pdf
        public async Task<string> GetPdf(BirokratDocumentType path, string sifra) {
            return await client.document.GetPdf(BironextApiPathHelper.GetVnosByType(path), sifra);
        }

        // fiskaliziraj
        public async Task<string> Fiscalize(BirokratDocumentType path, string sifra) {
            return await client.document.Fiscalize(BironextApiPathHelper.GetVnosByType(path), sifra);
        }

        // delete
        public async Task<string> Delete(BirokratDocumentType path, string sifra) {
            return await client.document.Delete(BironextApiPathHelper.GetVnosByType(path), sifra);
        }
    }
}
