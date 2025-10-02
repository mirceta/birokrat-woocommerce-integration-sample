using core.logic.common_birokrat;
using core.structs;
using core.tools.zalogaretriever;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWooHub.logic.integration
{
    public interface IBiroToWoo
    {
        BirokratField SkuBirokratField { get; set; }
        BirokratField VariableProductBirokratField { get; set; }
        Task OnArticleAdded(string sifra);
        Task OnArticleChanged(string sifra);
        Task OnArticleDeleted(string sifra);
        void SetBirokratArtikelRetriever(IBirokratArtikelRetriever zaloga);
        IBirokratArtikelRetriever GetBirokratArtikelRetriever();
        Dictionary<string, string> GetVariationAttributes();
    }

}
