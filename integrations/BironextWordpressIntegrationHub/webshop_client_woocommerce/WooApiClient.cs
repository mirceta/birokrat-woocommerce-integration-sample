using birowoo_exceptions;
using BiroWoocommerceHubTests;
using core.tools.wooops;
using Newtonsoft.Json;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using tests_fixture;
using transfer_data.sql_accessors.order_transfer_creator.deps;
using validator.logic.order_transfer.accessor;
using webshop_client_woocommerce.product_retriever;
using webshop_client_woocommerce.woo_caller;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.Legacy;

namespace webshop_client_woocommerce
{


    class WooSkuToProductMap { // not thread safe

        WooApiClient client;
        Dictionary<string, List<Dictionary<string, object>>> skuToProductMap;
        List<Dictionary<string, object>> products;
        
        public WooSkuToProductMap(WooApiClient client) {
            skuToProductMap = new Dictionary<string, List<Dictionary<string, object>>>();
            this.client = client;
        }

        public List<Dictionary<string, object>> Get(string sku) {
            if (skuToProductMap.Keys.Count == 0)
                FetchAll();
            return skuToProductMap[sku];
        }

        public bool ContainsKey(string sku) {
            if (skuToProductMap.Keys.Count == 0)
                FetchAll();
            return skuToProductMap.ContainsKey(sku);
        }

        public void RemoveKey(string sku) {
            if (skuToProductMap.Keys.Count == 0)
                FetchAll();
            if (skuToProductMap.ContainsKey(sku))
                skuToProductMap.Remove(sku);
            else
                throw new ProductNotFoundException("Izdelek ni bil najden");
        }

        public void EnsureExists(Dictionary<string, object> entry) {

            string sku = (string)entry["sku"];
            if (!skuToProductMap.ContainsKey(sku) || skuToProductMap[sku] == null)
            {
                skuToProductMap[sku] = new List<Dictionary<string, object>>();
            }

            var matches = skuToProductMap[sku].ToList().Where(x => GWooOps.SerializeIntWooProperty(x["id"]) == GWooOps.SerializeIntWooProperty(entry["id"])).ToList();
            if (matches.Count > 1)
                throw new Exception("THERE SHOULD BE ONLY ONE ENTRY WITH THIS ID!");
            if (matches.Count == 1)
                skuToProductMap[sku].Remove(matches[0]);

            skuToProductMap[sku].Add(entry);
        }

        public void RemoveById(string id)
        {
            // Define a function to match the entry by id
            Func<Dictionary<string, object>, bool> ismatch = (mtch) => GWooOps.SerializeIntWooProperty(mtch["id"]) == id;

            // Collect the keys and corresponding items to be removed
            var itemsToRemove = new List<(string sku, Dictionary<string, object> item)>();

            // Iterate over the dictionary to find matching items
            foreach (var kvp in skuToProductMap)
            {
                var matchingItems = kvp.Value.Where(ismatch).ToList();
                foreach (var item in matchingItems)
                {
                    itemsToRemove.Add((kvp.Key, item));
                }
            }

            // Remove the collected items after iteration
            foreach (var (sku, item) in itemsToRemove)
            {
                if (!skuToProductMap.TryGetValue(sku, out var entry))
                {
                    throw new InvalidOperationException($"Failed to find the SKU '{sku}' in the skuToProductMap.");
                }

                entry.Remove(item);

                if (entry.Count == 0)
                {
                    skuToProductMap.Remove(sku);
                }
            }
        }


        public List<Dictionary<string, object>> List() {
            if (skuToProductMap.Keys.Count == 0)
                FetchAll();
            return products;
        }
        
        public void FetchAll() {
            var retr = new WooProductRetriever(
                10,
                null);
            products = retr.Get(client);


            skuToProductMap = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (var prod in products) {
                string sku = prod["sku"] as string;
                if (!string.IsNullOrEmpty(sku)) {
                    if (!skuToProductMap.ContainsKey(sku)) {
                        skuToProductMap[sku] = new List<Dictionary<string, object>>();
                    }
                    skuToProductMap[sku].Add(prod);
                }
            }

            if (skuToProductMap.Count == 0) {
                skuToProductMap["-1-1-1-1"] = null;
            }
        }
    }

    public class WooApiClient : IOutApiClient {


        IWooApiCaller woocaller;
        WooSkuToProductMap skuToProductMap;

        RestAPI restApi;

        public WooApiClient(IWooApiCaller woocaller) {
            this.woocaller = woocaller;
            this.skuToProductMap = new WooSkuToProductMap(this);

            restApi = new RestAPI(woocaller.Address + "/wp-json/" + woocaller.Version, woocaller.Ck, woocaller.Cs);
            couponGetter = new CouponGetter(this);
        }

        si.birokrat.next.common.logging.IMyLogger logger;
        public void SetLogger(si.birokrat.next.common.logging.IMyLogger logger) {
            this.logger = logger;
            this.woocaller.SetLogger(logger);
        }
        
        public string Ck => woocaller.Ck;

        public string Cs => woocaller.Cs;

        public string Address => woocaller.Address;

        public async Task<string> Delete(string op) {
            return await woocaller.Delete(op);
        }

        public async Task<string> Get(string op) {
            op = ReplaceAmpersandPatterns(op);
            return await woocaller.Get(op);
        }

        

        public async Task<string> Post(string op, string body) {
            return await woocaller.Post(op, body);
        }

        public async Task<string> Put(string op, string body) {
            return await woocaller.Put(op, body);
        }

        #region [product modifiers]
        public async Task<Dictionary<string, object>> UpdateProduct(string id, Dictionary<string, object> values) {
            var wooload = values;

            /*
            foreach (var val in values) {
                wooload[map[val.param]] = val.value;
            }
            */

            string body = JsonConvert.SerializeObject(wooload);
            string res = await woocaller.Put($"products/{id}", body);

            GWooOps.ThrowExceptionIfProductPostWooApiCallFailed(body, res);

            File.WriteAllText("UpdateProduct_input.json", body);
            File.WriteAllText("UpdateProduct_return.json", res);

            return new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(res);
        }

        public async Task<Dictionary<string, object>> UpdateVariation(string product_id, string variation_id, Dictionary<string, object> values) {
            var wooload = values;

            /*foreach (var val in values) {
                wooload[map[val.param]] = val.value;
            }*/
            string body = JsonConvert.SerializeObject(wooload);
            string res = await woocaller.Put($"products/{product_id}/variations/{variation_id}", body);

            GWooOps.ThrowExceptionIfProductPostWooApiCallFailed(body, res);

            File.WriteAllText("UpdateProduct_input.json", body);
            File.WriteAllText("UpdateProduct_return.json", res);

            return new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(res);
        }

        public async Task<Dictionary<string, object>> PostProduct(Dictionary<string, object> product) {
            string woojson = JsonConvert.SerializeObject(product);
            string result = await woocaller.Post($"products", woojson);
            GWooOps.ThrowExceptionIfProductPostWooApiCallFailed(woojson, result);
            var tmp = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(result);

            File.WriteAllText("PostProduct_input.json", woojson);
            File.WriteAllText("PostProduct_return.json", result);

            skuToProductMap.EnsureExists(tmp);

            return tmp;
        }

        public async Task<Dictionary<string, object>> PostBaseVariableProduct(Dictionary<string, object> product) {
            var tmp = await PostProduct(product);
            skuToProductMap.EnsureExists(tmp);
            return tmp;
        }

        public async Task<Dictionary<string, object>> PostVariation(string parent_id, Dictionary<string, object> variation) {
            string json = JsonConvert.SerializeObject(variation);
            string res = await woocaller.Post($"products/{parent_id}/variations", json);
            GWooOps.ThrowExceptionIfProductPostWooApiCallFailed(json, res);
            var woovar = new JsonPowerDeserialization()
                .DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(res);

            File.WriteAllText("PostVariation_input.json", json);
            File.WriteAllText("PostVariation_return.json", res);

            skuToProductMap.EnsureExists(woovar);

            return woovar;

        }
        #endregion

        #region [product retrievers]
        public async Task<List<Dictionary<string, object>>> GetProducts() {
            skuToProductMap.FetchAll();
            return skuToProductMap.List();
        }
        
        public async Task<ProductResult> GetProductBySku(string sku) {
            if (!skuToProductMap.ContainsKey(sku))
            {
                return ProductResult.FailureResult("The product was not found");
            }

            var lst = skuToProductMap.Get(sku);

            Func<List<Dictionary<string, object>>, string> funcdesc = (lst) => {
                var ids = lst.Select(x => {
                    string parent = "";
                    string id = "";
                    if (x.ContainsKey("parent_id")) {
                        parent = x["parent_id"] as string;
                    }
                    id = x["id"] as string;
                    return $"Parent: {parent}, Id: {id}";
                }).ToList();
                return string.Join(", ", ids);
            };

            return ProductResult.SuccessResult(lst[0]);

            if (lst.All(x => x["status"] as string == "draft")) {
                string some = funcdesc(lst);
                throw new ProductInDraftStatusException($"For sku {sku} all products are in draft status: {some}");
            }
            
            lst = lst.Where(x => (string)x["status"] == "publish").ToList();
            
            if (lst.Count == 0)
                throw new ProductNotFoundException("There are no published products with this sku");
            if (lst.Count > 1) {
                throw new MultipleProductVariationsWithSameSku("Multiple published products found containing this sku " + funcdesc(lst));
            }
            return ProductResult.SuccessResult(lst[0]);
        }
        



        public async Task<List<Dictionary<string, object>>> GetVariableProducts() {
            // obsolete (unimportant)
            string products = await woocaller.Get($"products?type=variable'&'per_page=100");
            var prod = new JsonPowerDeserialization()
                    .DeserializeObjectImmuneToBadJSONEscapeSequenece<List<Dictionary<string, object>>>(products);
            return prod;
        }
        
        #endregion


        #region [others]
        public async Task<List<Dictionary<string, object>>> GetAttributes() {

            string res = "";
            try
            {
                res = await woocaller.Get("products/attributes");
                return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(res);
            }
            catch (Exception ex) {
                throw new Exception(res, ex);
            }
        }
        public async Task<List<Dictionary<string, object>>> GetAttributes(string productId) {

            string prod = "";

            try
            {
                prod = await woocaller.Get($"products/{productId}");
                var obj = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<TmpCls>(prod).Attributes;
                return obj;
            }
            catch (Exception ex) {
                throw new Exception(prod, ex);
            }
        }

        public async Task<List<Category>> GetCategories() {
            string cats = await woocaller.Get("products/categories?per_page=100");
            if (cats.Contains("\"code\":\"rest_invalid_param\""))
                cats = await woocaller.Get("products/categories");
            List<Category> some = new List<Category>();
            var deser = JsonConvert.DeserializeObject<List<Category>>(cats);
            return deser;
        }

        public async Task<Category> PostCategory(string name) {
            string json = await woocaller.Post("products/categories", $@"{{""name"": ""{name}""}}");
            var tp = JsonConvert.DeserializeObject<Category>(json);
            return tp;
        }

        public async Task<Dictionary<string, object>> PostAttribute(Dictionary<string, object> attributesBody) {
            string body = JsonConvert.SerializeObject(attributesBody);
            string res = await woocaller.Post("products/attributes", body);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
        }

        public async Task<List<Dictionary<string, object>>> GetAttributeTerms(string attributeId) {
            var some = await woocaller.Get($"products/attributes/{attributeId}/terms");
            var arr = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(some);
            return arr;
        }


        public async Task<Dictionary<string, object>> PostAttributeTerm(string attributeId, string term) {
            var some = woocaller.Post($"products/attributes/{attributeId}/terms", $@"{{""name"": ""{term}""}}");
            return null;
        }

        CouponGetter couponGetter;
        public async Task<string> MyGetOrder(string id) {
            return await new WooNativeRestApiOrderRetriever(this, couponGetter).GetOrder(id);
        }

        public async Task<List<string>> GetOrders(DateTime sinceDate)
        {
            var some = new WooOrderRetrieverHelper(maxDaysOld: DateTime.Now.Subtract(sinceDate).Days, wooclient: this);
            var someids = some.GetOrderIds();

            List<string> results = new List<string>();
            var oretr = new WooNativeRestApiOrderRetriever(this, couponGetter);
            foreach (var id in someids) {
                
                results.Add(await oretr.GetOrder(id));
            }

            return results;
        }

        public async Task<List<string>> GetOrderDescriptions(DateTime sinceDate)
        {
            int daysold = DateTime.Now.Subtract(sinceDate).Days;
            var some = new WooOrderRetrieverHelper(maxDaysOld: daysold, wooclient: this);
            return some.GetFullOrders();
        }

        public async Task<string> DeleteProduct(string id) {
            string some = await woocaller.Delete($"products/{id}");
            if (!some.Contains("<Response [200]>")) {
                throw new Exception("Fail!");
            }
            skuToProductMap.RemoveById(id);
            return some;
        }

        public async Task<string> DeleteProductBySku(string sku) {
            var some = await GetProductBySku(sku);
            var tmp = await DeleteProduct(GWooOps.SerializeIntWooProperty(some.Product["id"]));
            return tmp;
        }

        public async Task<string> DeleteVariation(string parent_id, string variation_id) {
            string some = await woocaller.Delete($"products/{parent_id}/variations/{variation_id}");
            if (!some.Contains("<Response [200]>")) {
                throw new Exception("Fail!");
            }
            skuToProductMap.RemoveById(GWooOps.SerializeIntWooProperty(variation_id));
            return some;
        }

        public async Task<string> PutKita(string query, string body) {
            return await woocaller.Put(query, body);
        }

        public async Task<string> PostKita(string query, string body)
        {
            return await woocaller.Post(query, body);
        }

        public async Task<string> DeleteKita(string query)
        {
            return await woocaller.Delete(query);
        }

        public async Task<string> GetKita(string query)
        {
            //ReplacementAmpersand is used for correct string formating. Before we had problem that solution worked on some computers and not on others
            query = ReplaceAmpersandPatterns(query);
            //return await restApi.GetRestful(query);
            return await woocaller.Get(query);
        }

        private string ReplaceAmpersandPatterns(string input)
        {
            //will replace every & or \'&\' or \"&\" to \\\"&\\\" if it not already in \\\"&\\\" format
            string replacement = $"\\\"&\\\"";
            string result;

            if (input.Contains(replacement))
                return input;

            string pattern2 = @"\'&\'";
            result = Regex.Replace(input, pattern2, replacement);
            if (input != result)
                return result;

            string pattern3 = @"\""&\""";
            result = Regex.Replace(input, pattern3, replacement);
            if (input != result)
                return result;

            return input.Replace("&", replacement);
        }

        public async Task<List<List<StatusChange>>> GetOrderStatusChanges(List<int> orderIds)
        {

            var x = new WooCommerceApiFetcher(this, 20);
            var notes = await x.FetchAllOrderNotes(orderIds);

            var statusChangeClass = new StatusChangeClass(this);
            return notes.Select(x => statusChangeClass.GetStatusChanges(x).OrderBy(x => x.EventDate).ToList()).ToList();
        }

        Dictionary<ProductParameter, string> map = new Dictionary<ProductParameter, string>() {
            { ProductParameter.GrossPrice, "regular_price"},
            { ProductParameter.NetPrice, null},
            { ProductParameter.SalePrice, "sale_price"}
        };
        #endregion
    }

    class Attribute {
        public string id;
        public string name;
        public string slug;
    }
    class TmpCls {
        public List<Dictionary<string, object>> Attributes;
    }
}
