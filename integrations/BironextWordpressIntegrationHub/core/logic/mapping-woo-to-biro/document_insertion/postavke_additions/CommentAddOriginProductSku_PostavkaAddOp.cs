using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.structs_wc_to_biro;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion.postavke_additions
{
    public class CommentAddOriginProductSku_PostavkaAddOp : IAdditionalOperationOnPostavke
    {

        bool append;
        public CommentAddOriginProductSku_PostavkaAddOp(bool append) {
            this.append = append;
        }

        public async Task<List<BirokratPostavka>> ApplyOperationToPostavke(WoocommerceOrder order, List<BirokratPostavka> postavke) {

            foreach (var postavka in postavke) {
                int idx = postavka.GetOriginalOrderIndex();
                var item = order.Items[idx];

                if (append) {
                    if (postavka.Comment == null)
                        postavka.Comment = "";
                    postavka.Comment += "\nSKU: " + ((string)item.OriginProduct["sku"]);
                } else {
                    postavka.Comment = "SKU: " + ((string)item.OriginProduct["sku"]);
                }
            }
            return postavke;
        }
    }
}
