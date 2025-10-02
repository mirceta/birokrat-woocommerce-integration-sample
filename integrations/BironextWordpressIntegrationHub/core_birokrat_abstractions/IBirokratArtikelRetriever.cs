using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.tools.zalogaretriever {
    public interface IBirokratArtikelRetriever {
        Task<Dictionary<string, object>> Build(string sifra);
        Task<List<Dictionary<string, object>>> Query(Dictionary<string, object> terms, Dictionary<string, object> parameters);
    }
}
