using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub;
using BiroWoocommerceHub.structs_wc_to_biro;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion.postavke_additions
{
    public class PriceMultiplierByVAT_ForOproscenaDobava_PostavkaAddOp : IAdditionalOperationOnPostavke {

        ICountryMapper mapper;
        IOrderAddOpCondition condition;
        BirokratPostavkaUtils utils;



        /*
         !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            WARNING WARNING WARNING WARNING WARNING
        !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        This is bad design, because IAdditionalOperationOnPostavke is always supposed to work on ALL postavke
        but should be applied before a percent coupon!

        When using this class always bear in mind that you will have to first apply it before applying either
        a percent coupon, and then after it only on the postavkas that matter! 
         
         
         
         
         
         */



        public PriceMultiplierByVAT_ForOproscenaDobava_PostavkaAddOp(BirokratPostavkaUtils utils, ICountryMapper mapper,
            IOrderAddOpCondition condition) {
            this.mapper = mapper;
            this.condition = condition;
            this.utils = utils;
        }

        public async Task<List<BirokratPostavka>> ApplyOperationToPostavke(WoocommerceOrder order, List<BirokratPostavka> postavke) {

            if (!this.condition.Is(order))
                return postavke;

            string cntry = await mapper.Map(order.Data.Billing.Country);

            foreach (var x in postavke) {
                double subtot = utils.GetSubtotal(x);
                subtot *= GetCountryToVAT(cntry);
                x.Subtotal = utils.SerializeToBirokratForm(subtot);
            }
            return postavke;
        }

        private double GetCountryToVAT(string cntry) {
            return 1.22;
        }
    }
}
