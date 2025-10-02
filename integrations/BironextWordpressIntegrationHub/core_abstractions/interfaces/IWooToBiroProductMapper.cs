using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.structs_wc_to_biro;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.customers.poledancerka.mappers
{
    public interface IWooToBiroProductMapper
    {
        bool IsThisTypeOfProduct(dynamic x);

         Task<BirokratPostavka> ProductItemToBirokratPostavka(WoocommerceOrderItem x, bool verifyAndCreate);

        Task MapWooProductToBirokrat(Dictionary<string, object> product);

        string GetOrAddProductAndReturnSifra();

    }
}
