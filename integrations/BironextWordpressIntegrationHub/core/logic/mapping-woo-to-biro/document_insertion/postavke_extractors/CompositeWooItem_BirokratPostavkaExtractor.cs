using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.structs_wc_to_biro;
using core.customers.poledancerka.mappers;
using core.logic.mapping_woo_to_biro.document_insertion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.customers.poledancerka
{
    public class CompositeWooItem_BirokratPostavkaExtractor : IBirokratPostavkaExtractor
    {
        IApiClientV2 client;
        List<IWooToBiroProductMapper> lst;
        bool verifyAndCreate;
        public CompositeWooItem_BirokratPostavkaExtractor(IApiClientV2 client, List<IWooToBiroProductMapper> lst, bool verifyAndCreate = true) {
            this.client = client;
            this.lst = lst;
            this.verifyAndCreate = verifyAndCreate;
        }

        public async Task<List<BirokratPostavka>> ExtractFromOrder(WoocommerceOrder order)
        {
            List<BirokratPostavka> postavke = new List<BirokratPostavka>();
            for (int i = 0; i < order.Items.Count; i++)
            {
                var postavka = await MapProductToBirokratPostavka(order.Items[i], verifyAndCreate, i);
                postavke.Add(postavka);
            }
            return postavke;
        }

        private async Task<BirokratPostavka> MapProductToBirokratPostavka(WoocommerceOrderItem item, bool verifyAndCreate, int idx)
        {
            foreach (var mapper in lst)
            {
                string some = JsonConvert.SerializeObject(item.OriginProduct);
                if (mapper.IsThisTypeOfProduct(item.OriginProduct))
                {
                    var postavka = await mapper.ProductItemToBirokratPostavka(item, verifyAndCreate);
                    postavka.SaveIndex_WooOrder(idx);
                    return postavka;
                }
            }
            throw new Exception("This item did not fit to any of the defined woo product types");
        }
    }
}
