using ApiClient.utils;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.structs_wc_to_biro;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BironextWordpressIntegrationHub
{

    public class SimplejsonGen
    {

        public static async Task<string> CreateJsonDocumentRequest(WoocommerceOrder order, List<BirokratPostavka> postavke, OrderAdditionalParams pars) {

            string tmpbillingcntry = order.Data.Billing.Country;
            string tmpshippingcntry = order.Data.Shipping.Country;
            if (pars.CountryMapper != null) {
                order.Data.Billing.Country = await pars.CountryMapper.Map(order.Data.Billing.Country);
                order.Data.Shipping.Country = await pars.CountryMapper.Map(order.Data.Shipping.Country);    
            }
            // restore woo counties
            

            string stevilkaRacuna = "";
            if (pars.SourceDocumentNumberExtractor != null) {
                stevilkaRacuna = (await pars.SourceDocumentNumberExtractor
                                    .GetDocumentNumber(BironextApiPathHelper.GetStringByType(pars.SourceDocumentType), order)).DocumentNumber;
            }

            foreach (var pos in postavke) {
                pos.Subtotal = Tools.SerializeDoubleToBirokratFormat(Tools.ParseDoubleBigBrainTime(pos.Subtotal));
            }
            

            var odr = SimplejsonOrder.FromWoocommerce(order, postavke, pars.AdditionalNumber, pars.ExternalUniqueIdentifier,
                BironextApiPathHelper.GetStringByType(pars.SourceDocumentType), stevilkaRacuna, pars.BirokratId);

            order.Data.Billing.Country = tmpbillingcntry;
            order.Data.Shipping.Country = tmpshippingcntry;

            return JsonConvert.SerializeObject(odr);
        }
    }
}
