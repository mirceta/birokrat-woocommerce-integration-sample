using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.structs_wc_to_biro;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion.postavke_extractors
{
    public class BirokratAttributeIsOriginalSku_BirokratPostavkaExtractor : IBirokratPostavkaExtractor
    {
        // WILL WORK FOR SIFRA, BARKODA, BARKODA3
        BirokratPostavkaUtils utils;
        public BirokratAttributeIsOriginalSku_BirokratPostavkaExtractor(BirokratPostavkaUtils utils) {
            this.utils = utils;
        }

        public async Task<List<BirokratPostavka>> ExtractFromOrder(WoocommerceOrder order) {
            List<BirokratPostavka> postavke = new List<BirokratPostavka>();
            for (int i = 0; i < order.Items.Count; i++) {
                var item = order.Items[i];
                BirokratPostavka postavka = null;
                postavka = GetSimpleBirokratPostavka(item);
                postavka.SaveIndex_WooOrder(i);
                postavke.Add(postavka);
            }
            return postavke;
        }

        private BirokratPostavka GetSimpleBirokratPostavka(WoocommerceOrderItem item) {
            string sku = ((string)item.OriginProduct["sku"]);
            var p = utils.Get(item, sku);
            return p;
        }
    }
}
