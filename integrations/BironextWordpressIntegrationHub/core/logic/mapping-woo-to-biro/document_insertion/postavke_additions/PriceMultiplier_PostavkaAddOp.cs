using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.structs_wc_to_biro;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion.postavke_additions {
    public class PriceMultiplier_PostavkaAddOp : IAdditionalOperationOnPostavke {

        double multiplier;
        IOrderAddOpCondition condition;
        BirokratPostavkaUtils utils;

        public PriceMultiplier_PostavkaAddOp(BirokratPostavkaUtils utils, double multiplier, IOrderAddOpCondition condition) {
            this.multiplier = multiplier;
            this.condition = condition;
            this.utils = utils;
        }

        public async Task<List<BirokratPostavka>> ApplyOperationToPostavke(WoocommerceOrder order, List<BirokratPostavka> postavke) {

            if (!condition.Is(order)) return postavke;
            
            foreach (var x in postavke) {
                double subtot = utils.GetSubtotal(x);
                subtot *= multiplier;
                x.Subtotal = utils.SerializeToBirokratForm(subtot);
            }
            return postavke;
        }
    }
}
