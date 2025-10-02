using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.structs_wc_to_biro;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion.postavke_extractors
{
    public class BirokratAttributeIsOriginalOrVariationSku_BirokratPostavkaExtractor : IBirokratPostavkaExtractor
    {
        // WILL WORK FOR SIFRA, BARKODA, BARKODA3
        BirokratPostavkaUtils utils;
        public BirokratAttributeIsOriginalOrVariationSku_BirokratPostavkaExtractor(BirokratPostavkaUtils utils) {
            this.utils = utils;
        }

        public async Task<List<BirokratPostavka>> ExtractFromOrder(WoocommerceOrder order)
        {
            List<BirokratPostavka> postavke = new List<BirokratPostavka>();
            for (int i = 0; i < order.Items.Count; i++) 
            {
                var item = order.Items[i];
                BirokratPostavka postavka = null;

                if (item.OriginProduct["variations"] != null && item.OriginProduct["variations"].Count > 0)
                {
                    postavka = GetVariationBirokratPostavka(item);
                    postavka.SaveIndex_WooOrder(i);
                }
                else
                {
                    postavka = GetSimpleBirokratPostavka(item);
                    postavka.SaveIndex_WooOrder(i);
                }
                postavke.Add(postavka);
            }
            return postavke;
        }

        private BirokratPostavka GetVariationBirokratPostavka(WoocommerceOrderItem item)
        {
            string sku = item.Sku;
            var p = utils.Get(item, sku);
            return p;
        }

        private BirokratPostavka GetSimpleBirokratPostavka(WoocommerceOrderItem item)
        {
            string sku = ((string)item.OriginProduct["sku"]);
            var p = utils.Get(item, sku);
            return p;
        }
    }
}
