using core.tools.attributemapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiroWoocommerceHubTests.tools {
    public interface IBiroProductToOutMapper {
        IBiroProductToOutMapper SetZaloga(bool includeZaloga);
        IBiroProductToOutMapper SetTax(BiroTaxToWooTax tax);

        IBiroProductToOutMapper AddVariationDeterminant(string birokratProperty);
        IBiroProductToOutMapper AddMapping(string birokratProperty, string wooProperty);
        IBiroProductToOutMapper AddCategoryMapping(string birokratProperty);
        IBiroProductToOutMapper AddAttributeMapping(string birokratProperty, WooAttr wooAttribute);

        Dictionary<string, string> GetAttributeMappings();

        Task<Dictionary<string, object>> Map(Dictionary<string, object> biroArtikel);
    }
}