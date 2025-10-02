using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.structs_wc_to_biro;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion
{
    public interface IBirokratPostavkaExtractor {
        Task<List<BirokratPostavka>> ExtractFromOrder(WoocommerceOrder order);
    }

}
