using birowoo_exceptions;
using BiroWoocommerceHub.flows;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Text;

namespace core.logic.mapping_biro_to_woo.syncers {
    class BiroProductSyncerHelper {
        public static bool areTheSame(Dictionary<string, object> biroArtikel, string product) {

            var wooprod = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(product);

            int woostock = (int)Tools.ParseDoubleBigBrainTime(GWooOps.SerializeIntWooProperty(wooprod["stock_quantity"]));



            string woopriceattr = "regular_price";
            if (GWooOps.SerializeDblWooProperty(wooprod["regular_price"]) == "0") {
                woopriceattr = "price";
            }
            double wooprice = Tools.ParseDoubleBigBrainTime(GWooOps.SerializeDblWooProperty(wooprod[woopriceattr]));



            int birozaloga = (int)Tools.ParseDoubleBigBrainTime((string)biroArtikel["zaloga"]);
            double biroprice = Tools.ParseDoubleBigBrainTime((string)biroArtikel["PCsPD"]);

            if (woostock != birozaloga || biroprice != wooprice) {
                throw new ProductStillDifferentThanArtikelAfterUpdateException("Velik problem");
            }

            return true;
        }

        public static bool separatorNotRecognized(Dictionary<string, object> biroArtikel, string product) {

            var wooprod = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(product);
            
            string woopriceattr = "regular_price";
            if (GWooOps.SerializeDblWooProperty(wooprod["regular_price"]) == "0") {
                woopriceattr = "price";
            }
            double wooprice = Tools.ParseDoubleBigBrainTime(GWooOps.SerializeDblWooProperty(wooprod[woopriceattr]));
            
            double biroprice = Tools.ParseDoubleBigBrainTime((string)biroArtikel["PCsPD"]);

            if (Math.Abs(100.0 * biroprice - wooprice) < 0.1) {
                throw new ProductStillDifferentThanArtikelAfterUpdateException("Separator ni bil razpoznan! Cena je 100x vecja!");
            }

            return true;
        }
    }
}
