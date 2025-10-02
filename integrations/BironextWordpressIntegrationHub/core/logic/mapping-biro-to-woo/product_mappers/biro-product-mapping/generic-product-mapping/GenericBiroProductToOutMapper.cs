using BirokratNext;
using birowoo_exceptions;
using BiroWoocommerceHubTests.tools;
using core.tools.attributemapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_biro_to_woo.tools.biro_product_mapping.generic_product_mapping {
    public class GenericBiroProductToOutMapper : IBiroProductToOutMapper {

        bool includeZaloga = false;
        BiroTaxToWooTax tax;

        Dictionary<string, string> biroToWooMap;
        Dictionary<string, WooAttr> biroToWooAttributeMap;
        List<string> categoryAttributes;
        Dictionary<string, object> categoryMap;

        string variationDeterminant;

        public GenericBiroProductToOutMapper() {
            biroToWooMap = new Dictionary<string, string>();
            biroToWooAttributeMap = new Dictionary<string, WooAttr>();
            categoryMap = new Dictionary<string, object>();
            categoryAttributes = new List<string>();
        }

        #region [add mappings]
        public IBiroProductToOutMapper AddVariationDeterminant(string birokratProperty) {
            this.variationDeterminant = birokratProperty;
            return this;
        }
        public IBiroProductToOutMapper AddMapping(string birokratProperty, string wooProperty) {
            biroToWooMap.Add(birokratProperty, wooProperty);
            return this;
        }
        public IBiroProductToOutMapper AddCategoryMapping(string birokratProperty) {
            categoryAttributes.Add(birokratProperty);
            return this;
        }
        public IBiroProductToOutMapper AddAttributeMapping(string birokratProperty, WooAttr wooAttribute) {
            biroToWooAttributeMap.Add(birokratProperty, wooAttribute);
            return this;
        }
        #endregion

        public Dictionary<string, string> GetAttributeMappings() {
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (var item in biroToWooAttributeMap) {
                map.Add(item.Key, item.Value.Name);
            }
            return map;
        }

        public async Task<Dictionary<string, object>> Map(Dictionary<string, object> biroArtikel) {
            var intermediate = new Dictionary<string, object>();

            if (tax != null)
                intermediate = tax.Map(intermediate, biroArtikel);
            intermediate = MapZaloga(intermediate, biroArtikel);
            intermediate = MapRegularProperties(intermediate, biroArtikel);
            intermediate = MapAttributes(intermediate, biroArtikel);
            intermediate = MapCategories(intermediate, biroArtikel);

            if (!string.IsNullOrEmpty((string)biroArtikel[variationDeterminant])) {
                intermediate["variant"] = (string)biroArtikel[variationDeterminant];
            }

            return intermediate;
        }

        public IBiroProductToOutMapper SetTax(BiroTaxToWooTax tax) {
            this.tax = tax;
            return this;
        }

        public IBiroProductToOutMapper SetZaloga(bool includeZaloga) {
            this.includeZaloga = includeZaloga;
            return this;
        }


        #region [auxiliary]
        private Dictionary<string, object> MapRegularProperties(Dictionary<string, object> woojson, Dictionary<string, object> biroArtikel) {
            foreach (var key in biroToWooMap.Keys) {
                woojson[biroToWooMap[key]] = biroArtikel[key];
            }
            return woojson;
        }
        private Dictionary<string, object> MapZaloga(Dictionary<string, object> woojson, Dictionary<string, object> biroArtikel) {
            string zaloga = (string)biroArtikel["zaloga"];
            if (string.IsNullOrEmpty(zaloga)) {
                woojson["stock_quantity"] = "0";
                woojson["manage_stock"] = true;
            } else {
                woojson["stock_quantity"] = zaloga;
                woojson["manage_stock"] = true;
            }
            return woojson;
        }

        private Dictionary<string, object> MapAttributes(Dictionary<string, object> woojson, Dictionary<string, object> biroArtikel) {
            var wooattrs = new Dictionary<string, object>();
            foreach (var key in biroToWooAttributeMap.Keys) {
                string value = (string)biroArtikel[key];
                if (!string.IsNullOrWhiteSpace(value)) {
                    
                    WooAttr wooattribute = biroToWooAttributeMap[key];



                    if (wooattribute.Mandatory && string.IsNullOrEmpty(value)) {
                        throw new IntegrationProcessingException($"Article did not contain the value of attribute {wooattribute.Name}");
                    }
                    wooattrs[wooattribute.Name] = value;
                }
            }
            woojson["attributes"] = wooattrs;
            return woojson;
        }

        private Dictionary<string, object> MapCategories(Dictionary<string, object> woojson, Dictionary<string, object> biroArtikel) {
            return woojson;
        }
        
        #endregion
    }
}
