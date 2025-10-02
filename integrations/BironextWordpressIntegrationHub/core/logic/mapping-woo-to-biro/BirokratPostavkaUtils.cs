using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.structs_wc_to_biro;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace core.logic.mapping_woo_to_biro {
    public class BirokratPostavkaUtils {


        bool use_total_insteadofsubtotal = false;
        bool include_tax = false;
        public BirokratPostavkaUtils(bool use_total_insteadofsubtotal, bool include_tax = true) {
            this.use_total_insteadofsubtotal = use_total_insteadofsubtotal;
            this.include_tax = include_tax;
        }


        public double GetSubtotal(BirokratPostavka pos) {
            string some = pos.Subtotal.Replace(".", ",");
            CultureInfo culture = new CultureInfo("de"); // de culture means '.' is thousands sep, ',' is decimal sep
            double ret = double.Parse(some, culture);
            return ret;
        }

        public string SerializeToBirokratForm(double value) {
            CultureInfo culture = new CultureInfo("de"); // de culture means '.' is thousands sep, ',' is decimal sep
            string some = value.ToString("0.00000", culture);
            return some.Replace(",", ".");
        }

        public BirokratPostavka Get(WoocommerceOrderItem x, string BirokratSifra) {
            string subtotal = WooFormToBirokratForm(x.Subtotal, x.SubtotalTax);

            if (use_total_insteadofsubtotal) {
                subtotal = WooFormToBirokratForm(x.Total, x.TotalTax);
            }

            var p = new BirokratPostavka() {
                BirokratSifra = BirokratSifra,
                Quantity = x.Quantity,
                Subtotal = subtotal,
            };
            return p;
        }

        private string WooFormToBirokratForm(string value, string tax) {
            double val1 = Tools.ParseDoubleBigBrainTime(value.Replace(".", ","));
            double tax1 = (string.IsNullOrEmpty(tax) ? 0 : Tools.ParseDoubleBigBrainTime(tax.Replace(".", ",")));

            val1 = Math.Round(val1 * 100.0) / 100.0;
            tax1 = Math.Round(tax1 * 100.0) / 100.0;
            
            if (!include_tax) tax1 = 0;

            string ret = string.Format("{0:0.000000}", ((val1 + tax1)));
            return ret.Replace(",", ".");
        }
    }
}
