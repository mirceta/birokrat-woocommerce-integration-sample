using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.logic;
using BiroWoocommerceHub.structs_wc_to_biro;
using core.tools.birokratops;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion
{
    public class CouponFixedCart_PostavkeAddOp : IAdditionalOperationOnPostavke
    {

        IApiClientV2 client;
        List<IAdditionalOperationOnPostavke> operationsOnlyOnFixedCartCoupon;
        public CouponFixedCart_PostavkeAddOp(IApiClientV2 client, List<IAdditionalOperationOnPostavke> operationsOnlyOnFixedCartCoupon = null) {
            this.client = client;
            this.operationsOnlyOnFixedCartCoupon = operationsOnlyOnFixedCartCoupon;
        }

        public async Task<List<BirokratPostavka>> ApplyOperationToPostavke(WoocommerceOrder order, List<BirokratPostavka> postavke) {
            List<dynamic> some = new List<dynamic>();
            for (int i = 0; i < order.UsedCouponsCodes.ToArray().Length; i++) {
                for (int j = 0; j < order.Coupons.Count; j++) {
                    string usedcode = order.UsedCouponsCodes[i];
                    string code = (string)order.Coupons[j]["code"];
                    if (usedcode == code) {
                        some.Add(order.Coupons[j]);
                    }
                }
            }
            var usedCoupons = some;
            var postavkeNew = new List<BirokratPostavka>();
            usedCoupons = usedCoupons.Where(x => (string)x["discount_type"] == "fixed_cart").ToList();

            if (usedCoupons.Count == 0)
                return postavke;

            foreach (var x in usedCoupons) {
                var postavka = new BirokratPostavka() {
                    BirokratSifra = "",
                    Quantity = 1,
                    Subtotal = "-" + x["amount"],
                    Comment = "Koda kupona: " + (string)x["code"]
                };

                var args = new SearchThenIfNotFoundCreateArgs() {
                    sifrantpath = @"sifranti/artikli/prodajniartikli-storitve",
                    searchterm = "VREDKUP06",
                    nameoffieldtocomparewith = "txtSifraArtikla",
                    valuetocomparewith = "VREDKUP06",
                    pack = new Dictionary<string, object>() {
                        { "txtOpis", "Use fixed cart coupon" },
                        { "txtSifraArtikla", "VREDKUP06"},
                        { "SifraDavka", "1    22 DDV osnovna stopnja" },
                        { "txtEnota", "kos"}
                    },
                    fieldtoreturn = "txtSifraArtikla"
                };
                string sifra = await new ClassicBirokratSifrantPersistor(client, null, new BirokratArtikelRetrieverSearchStrategy(client)).SearchThenIfNotFoundCreate(args);
                postavka.BirokratSifra = sifra;

                postavkeNew.Add(postavka);
            }

            if (operationsOnlyOnFixedCartCoupon != null)
            {
                foreach (var op in operationsOnlyOnFixedCartCoupon)
                {
                    postavkeNew = await op.ApplyOperationToPostavke(order, postavkeNew);
                }
            }


            // COUPON CANNOT BE WORTH MORE THAN THE SUM OF ALL POSTAVKE!
            EnsureCouponsAreNotWorthMoreThanWholeOrder(postavke, postavkeNew);

            postavke.AddRange(postavkeNew);


            return postavke;
        }

        private static void EnsureCouponsAreNotWorthMoreThanWholeOrder(List<BirokratPostavka> postavke, List<BirokratPostavka> postavkeNew) {
            double neki = 0;
            foreach (var postavka in postavke) {
                neki += Tools.ParseDoubleBigBrainTime(postavka.Subtotal);
            }


            bool nullallahead = false;
            foreach (var postavka in postavkeNew) {

                if (nullallahead) {
                    postavka.Subtotal = "0";
                    continue;
                }

                var k1 = Math.Abs(Tools.ParseDoubleBigBrainTime(postavka.Subtotal));
                if (k1 > neki) {
                    postavka.Subtotal = "-" + Tools.SerializeDoubleToBirokratFormat(neki);
                    nullallahead = true;
                } else {
                    neki -= k1;
                }
            }
        }
    }
}
