using System;
using System.Collections.Generic;
using System.Text;

namespace core.tools.attributemapper
{
    public class BiroTaxToWooTax
    {

        Dictionary<string, string> mapping;
        string biroArtikelTaxField;
        string wooProductTaxField;
        public BiroTaxToWooTax(string biroArtikelTaxField, string wooProductTaxField) {
            this.biroArtikelTaxField = biroArtikelTaxField;
            this.wooProductTaxField = wooProductTaxField;
            mapping = new Dictionary<string, string>();
        }

        public BiroTaxToWooTax AddMapping(string biroFieldValue, string wooFieldValue) {
            mapping[biroFieldValue] = wooFieldValue;
            return this;
        }

        public Dictionary<string, object> Map(Dictionary<string, object> woojson, Dictionary<string, object> biroArtikel) {
            string birotaxval = ((string)biroArtikel[biroArtikelTaxField]).Trim();
            string wootaxval = mapping[birotaxval];
            woojson[wooProductTaxField] = wootaxval;
            return woojson;
        }
    }
}
