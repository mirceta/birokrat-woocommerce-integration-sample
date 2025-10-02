using BironextWordpressIntegrationHub.structs;
using birowoo_exceptions;
using BiroWoocommerceHubTests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.tools.wooops
{
    public class GWooOps
    {

        public static DateTime ParseWooDate(string date) {
            return DateTime.ParseExact(date.Substring(0, date.IndexOf(" ")).Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public static bool IsVatExempt(WoocommerceOrder order) {
            bool isvatexempt = false;
            var some = order.Data.MetaData.Where(x => x.Key == "is_vat_exempt").ToList();
            if (some.Count > 0) {
                string isVatExempt = Convert.ToString(some.First().Value);

                if (isVatExempt == "yes")
                    isvatexempt = true;
            }
            double sum = 0;
            foreach (var item in order.Items) {
                sum += double.Parse(item.SubtotalTax) + double.Parse(item.TotalTax);
            }
            if (sum == 0) {
                if (sum != 0 && isvatexempt == true) {
                    throw new Exception("The order said that vat exempt is true, but the sum of VATs was not 0");
                }
                isvatexempt = true;
            }

            return isvatexempt;
        }

        public static void SetSalePrice_ByProductId(IOutApiClient integ, string sale_price, string productId) {
            // sale price setting
            /*
            var wooload = new Dictionary<string, object>();
            wooload["sale_price"] = sale_price;
            string body = JsonConvert.SerializeObject(wooload);
            string res = integ.Put($"products/{productId}", body);

            GWooOps.ThrowExceptionIfProductPostWooApiCallFailed(body, res);
            */

            var wooload = new Dictionary<string, object>();
            wooload["sale_price"] = sale_price;
            integ.UpdateProduct(productId, wooload);
        }

        public static void SetSalePrice_Variation_ByProductId(IOutApiClient integ, string sale_price, string productId, string variationid) {
            /*
            var wooload = new Dictionary<string, object>();
            wooload["sale_price"] = sale_price;
            string body = JsonConvert.SerializeObject(wooload);
            string res = integ.Put($"products/{productId}/variations/{variationid}", body);

            GWooOps.ThrowExceptionIfProductPostWooApiCallFailed(body, res);
            */

            var wooload = new Dictionary<string, object>();
            wooload["sale_price"] = sale_price;
            integ.UpdateVariation(productId, variationid, wooload);
        }

        public static List<KeyValuePair<string, string>> ForVariation_WithSku_GetAttributeKeyValuePairs(dynamic product, string sku) {
            if (product["variations"] == null)
                return new List<KeyValuePair<string, string>>();
            Func<object, string> identity = (x) => (string)x;
            int idx = GetIdxOfVariationWithValue(product, "sku", identity, sku);
            return ForVariation_WithIdx_GetAttributeKeyValuePairs(product, idx);
        }

        public static List<KeyValuePair<string, string>> ForVariation_WithId_GetAttributeKeyValuePairs(dynamic product, int variation_id) {
            if (product["variations"] == null)
                return new List<KeyValuePair<string, string>>();
            Func<object, string> variationIdToString = (x) => SerializeIntWooProperty(x);
            int idx = GetIdxOfVariationWithValue(product, "variation_id", variationIdToString, "" + variation_id);
            return ForVariation_WithIdx_GetAttributeKeyValuePairs(product, idx);
        }

        private static int GetIdxOfVariationWithValue(dynamic product, string field, Func<object, string> stringTransform, string value) {
            string some = JsonConvert.SerializeObject(product["variations"]);
            for (int j = 0; j < product["variations"].Count; j++) {
                if (product["variations"][j][field] == value) {
                    return j;
                }
            }
            throw new IntegrationProcessingException($"During attempt to get idx of variation with {field} = {value}. No matching variation was found!");
        }


        private static List<KeyValuePair<string, string>> ForVariation_WithIdx_GetAttributeKeyValuePairs(dynamic product, int variationidx) {

            string some = JsonConvert.SerializeObject(product["variations"]);
            var variation = product["variations"][variationidx];

            List<KeyValuePair<string, string>> vals = new List<KeyValuePair<string, string>>();
            if (variation["attributes"].Count != null) { // is an array
                for (int i = 0; i < variation["attributes"].Count; i++) {
                    string key = variation["attributes"][i]["key"];
                    key = key.Replace("attribute_pa", "");
                    string value = variation["attributes"][i]["value"];
                    vals.Add(new KeyValuePair<string, string>(key, value));
                }
            } else { // is a dictionary
                foreach (var package in variation["attributes"]) {
                    string key = package.Name;
                    string value = variation["attributes"][key];
                    key = key.Replace("attribute_pa", "").Replace("_", "");
                    key = key.Replace("-", " ");
                    vals.Add(new KeyValuePair<string, string>(key, value));
                }
            }
            return vals;
        }



        public static int OriginProductAttributeCount(dynamic x)
        {
            if (x["variations"] != null &&
                RetardedDynamicLength(x["variations"]) > 0 &&
                x["variations"][0]["attributes"] != null) {

                var obj = x["variations"][0]["attributes"];
                int len = RetardedDynamicLength(obj);
                return len;
            } else {
                return 0;
            }
        }
        public static string SerializeIntWooProperty(object wooval) {
            if (wooval == null) return "0";
            string id = "";
            if (wooval is string) {
                if ((string)wooval == "") return "0";
                id = (string)wooval;
            } else if (wooval is JValue) {
                return ((JValue)wooval).Value.ToString();
            } else if (wooval is int) {
                return ((int)wooval).ToString();
            } else {
                id = ((Int64)wooval).ToString();
            }
            return id;
        }

        public static string SerializeDblWooProperty(object wooval) {
            if (wooval == null) return "0";
            string id = "";
            if (wooval is string)
            {
                if ((string)wooval == "") return "0";
                id = (string)wooval;
            }
            else if (wooval is JValue)
            {
                return ((JValue)wooval).Value.ToString();
            }
            else if (wooval is double)
            {
                return ((double)wooval).ToString();
            }
            else if (wooval is decimal)
            {
                return wooval.ToString();
            }
            else if (wooval is int) {
                id = (int)wooval + "";
            }
            else
            {
                id = ((Int64)wooval).ToString();
            }
            return id;
        }

        public static async Task<Dictionary<string, object>> GetWooProductBySku(IOutApiClient woo, string sku)
        {
            /*
            string ok = TryGetWooProductIdFromSku(woo, sku);
            ok = woo.Get($"products/{ok}");
            var tmp = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(ok);
            if ((string)tmp["sku"] == sku)
            {
                return tmp;
            }
            else {
                throw new Exception("The product was not found!");
            }
            */
            return (await woo.GetProductBySku(sku)).Product;
        }

        public static void ThrowExceptionIfProductPostArrayWooApiCallFailed(string req, string res) {
            // handle invalid woo call exception
            var x = new JsonPowerDeserialization2().DeserializeObjectImmuneToBadJSONEscapeSequenece<List<Dictionary<string, object>>>(res);
            try {
                if (res.Length < 10 && res.Contains("[]")) return;
                if (x.Count > 0) {
                    ThrowExceptionIfProductPostWooApiCallFailed(req, JsonConvert.SerializeObject(x[0]));
                    return;
                }
                throw new Exception("");
            } catch (Exception ex) {
                throw new WooCallFailException("Response: " + res + "Request: " + req + ex.Message);
            }
        }

        public static void ThrowExceptionIfProductPostWooApiCallFailed(string req, string res) {
            // handle invalid woo call exception
            var x = new JsonPowerDeserialization2().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(res);
            try {
                string t = GWooOps.SerializeIntWooProperty(x["id"]);
            } catch (Exception ex) {
                throw new WooCallFailException("Response: " + res + "Request: " + req + ex.Message);
            }
        }

        public static void ThrowExceptionIfProductPostWooApiCallFailedUseJsonPowerDeserializer(string req, string res) {
            // handle invalid woo call exception
            var x = new JsonPowerDeserialization2().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(res);
            try {
                string t = GWooOps.SerializeIntWooProperty(x["id"]);
            } catch (Exception ex) {
                throw new WooCallFailException("Response: " + res + "Request: " + req);
            }
        }



        public static dynamic GetVariation(WoocommerceOrderItem item) {
            int finalvarid = -1;
            for (int j = 0; j < item.OriginProduct["variations"].Count; j++) {
                int varid = item.OriginProduct["variations"][j]["variation_id"];
                if (varid == item.VariationId) {
                    finalvarid = j;
                    break;
                }
            }
            if (finalvarid == -1)
                throw new VariationNotFoundException(item.VariationId, int.Parse(GWooOps.SerializeIntWooProperty(item.OriginProduct["id"])));
            var variation = item.OriginProduct["variations"][finalvarid];
            return variation;
        }

        public static int RetardedDynamicLength(dynamic dyn) {
            int cnt = 0;
            foreach (var package in dyn) {
                cnt++;
            }
            return cnt;
        }
    }
}
