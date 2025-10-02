using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.structs_wc_to_biro;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion
{
    public class CouponPercent_PostavkeAddOp : IAdditionalOperationOnPostavke
    {
        public async Task<List<BirokratPostavka>> ApplyOperationToPostavke(WoocommerceOrder order, List<BirokratPostavka> postavke)
        {

            if (order.Coupons.Count == 0 && order.UsedCouponsCodes.Count != 0) {
                return WithoutCouponsFeatureInWoo(order, postavke);
            } else {
                return WithCouponsFeatureInWoo(order, postavke);
            }

        }

        List<BirokratPostavka> WithCouponsFeatureInWoo(WoocommerceOrder order, List<BirokratPostavka> postavke) {
            int cumulativePercentOff = 0;

            for (int i = 0; i < order.Coupons.Count; i++) {
                var coupon = order.Coupons[i];
                var some = (string)coupon["code"];
                if (order.UsedCouponsCodes.Contains(some) && coupon["discount_type"] == "percent") {
                    try
                    {
                        cumulativePercentOff += int.Parse(GWooOps.SerializeIntWooProperty(coupon["amount"]));
                    }
                    catch (Exception EX) {
                        cumulativePercentOff += (int)Tools.ParseDoubleBigBrainTime((string)coupon["amount"]);
                    }
                }
            }

            // apply coupon
            postavke = postavke.Select(x => {
                x.DiscountPercent = cumulativePercentOff;
                return x;
            }).ToList();

            return postavke;
        }

        List<BirokratPostavka> WithoutCouponsFeatureInWoo(WoocommerceOrder order, List<BirokratPostavka> postavke) {

            for (int i = 0; i < order.Items.Count; i++) {
                double subtotal = Tools.ParseDoubleBigBrainTime(order.Items[i].Subtotal);
                double total = Tools.ParseDoubleBigBrainTime(order.Items[i].Total);
                if (subtotal - total  > 0) {
                    double percent = Math.Round(((subtotal - total) / subtotal) * 100);
                    postavke[i].DiscountPercent = (int)percent;
                }
            }
            return postavke;

        }
    }
}
