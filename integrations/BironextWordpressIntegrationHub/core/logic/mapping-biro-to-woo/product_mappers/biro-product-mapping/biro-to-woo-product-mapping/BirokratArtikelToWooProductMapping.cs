using BirokratNext;
using birowoo_exceptions;
using core.logic.mapping_biro_to_woo;
using core.tools;
using core.tools.attributemapper;
using core.tools.wooops;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BiroWoocommerceHubTests.tools {

    public enum WooProductType { 
        SIMPLE,
        VARIABLE
    }

    public class ArtikelToProductMapping
    {

        IApiClientV2 client;
        IOutApiClient wooclient;
        Dictionary<string, string> mappings;
        List<string> categoryMappings;
        Dictionary<string, WooAttr> attributeMappings;
        BiroTaxToWooTax taxMapping;
        bool zaloga;
        WooProductType type;


        private BirokratArtikelToWooProductMapping _mapping;

        private ArtikelToProductMapping(IApiClientV2 client, 
            IOutApiClient wooclient, 
            Dictionary<string, string> mappings, 
            List<string> categoryMappings, 
            Dictionary<string, WooAttr> attributeMappings, 
            BiroTaxToWooTax taxMapping, 
            bool zaloga, 
            WooProductType type)
        {
            this.client = client;
            this.wooclient = wooclient;
            this.mappings = mappings;
            this.categoryMappings = categoryMappings;
            this.attributeMappings = attributeMappings;
            this.taxMapping = taxMapping;
            this.zaloga = zaloga;
            this.type = type;
        }
        //static async Task NullObject()
        //{
        //    await Setup();
        //}


        bool setupCalled = false;
        public async Task Setup([CallerMemberName] string callerName = "") { 
            StackTrace stackTrace = new StackTrace();
            StackFrame callingFrame = stackTrace.GetFrame(1);
            if (callerName == "NullObject")
                return;

            _mapping = new BirokratArtikelToWooProductMapping(client, wooclient)
            .SetType(type)
            .SetZaloga(zaloga)
            .SetTax(taxMapping);
            foreach (var (birokratAttr, wooAttr) in mappings)
            {
                _mapping.AddMapping(birokratAttr, wooAttr);
            }

            foreach (var category in categoryMappings)
            {
                _mapping.AddCategoryMapping(category);
            }

            // Assume attributeMappings follow similar pattern as mappings
            foreach (var (birokratAttr, wooAttr) in attributeMappings)
            {
                await _mapping.AddAttributeMapping(birokratAttr, wooAttr);
            }

            setupCalled = true;
        }

        public static async Task<ArtikelToProductMapping> NullObject() {
            var tmp = new ArtikelToProductMapping(null, null, null, null, null, null, false, WooProductType.SIMPLE);
            await tmp.Setup();
            return tmp;
        }

        public ArtikelToProductMapping SetCustom(BirokratArtikelToWooProductMapping mapping) {
            this._mapping = mapping;

            this.mappings = mapping.GetMappings();
            this.categoryMappings = mapping.GetCategoryAttributes();
            this.attributeMappings = mapping.GetAttributeMappingsFull();
            this.taxMapping = mapping.GetTax();
            this.zaloga = mapping.GetZaloga();
            this.type = mapping.GetProductType();
            this.client = mapping.getClient();
            this.wooclient = mapping.GetOutApiClient();

            return this;
        }

        public async Task<Dictionary<string, object>> CompleteProductMapping(Dictionary<string, object> biroArtikel)
        {
            return await _mapping.CompleteProductMapping(biroArtikel);
        }

        public async Task<Dictionary<string, object>> CompleteVariationMapping(Dictionary<string, object> biroArtikel, string parentId, ArtikelToProductMapping baseProductMapping)
        {
            return await _mapping.CompleteVariationMapping(biroArtikel, parentId, baseProductMapping);
        }

        public Dictionary<string, string> GetAttributeMappings()
        {
            return _mapping.GetAttributeMappings();
        }

        public dynamic[] MapAttributesAndEnsureGlobalAttributesAndTermsUploaded(Dictionary<string, object> biroArtikel)
        {
            return _mapping.MapAttributesAndEnsureGlobalAttributesAndTermsUploaded(biroArtikel);
        }

        public void AppendAttrTermsToProductAttributeDomain(Dictionary<string, object> biroArtikel, string productId) {
            _mapping.AppendAttrTermsToProductAttributeDomain(biroArtikel, productId);
        }
    }

    public class BirokratArtikelToWooProductMapping
    {

        WooProductType wooProductType = WooProductType.SIMPLE;
        bool includeZaloga = false;
        BiroTaxToWooTax tax;

        Dictionary<string, string> biroToWooMap;
        Dictionary<string, WooAttr> biroToWooAttributeMap;
        List<string> categoryAttributes;
        Dictionary<string, object> categoryMap;

        AttributeMapper attributeMapper = null;

        IApiClientV2 client;
        IOutApiClient wooclient;

        // need to keep which Birokrat attributes go to which Woocommerce attributes!

        public BirokratArtikelToWooProductMapping(IApiClientV2 client, IOutApiClient wooclient)
        {

            this.client = client;
            this.wooclient = wooclient;

            biroToWooMap = new Dictionary<string, string>();
            biroToWooAttributeMap = new Dictionary<string, WooAttr>();
            categoryMap = new Dictionary<string, object>();
            categoryAttributes = new List<string>();
        }

        #region [properties]
        public BirokratArtikelToWooProductMapping SetType(WooProductType wpt)
        {
            this.wooProductType = wpt;
            return this;
        }

        public BirokratArtikelToWooProductMapping SetZaloga(bool includeZaloga)
        {
            this.includeZaloga = includeZaloga;
            return this;
        }

        public BirokratArtikelToWooProductMapping SetTax(BiroTaxToWooTax tax)
        {
            this.tax = tax;
            return this;
        }
        #endregion


        public IApiClientV2 getClient() {
            return this.client;
        }

        public IOutApiClient GetOutApiClient() {
            return this.wooclient;
        }
        public Dictionary<string, string> GetAttributeMappings()
        {
            return biroToWooAttributeMap.ToDictionary(x => x.Key, x => x.Value.Name);
        }

        public Dictionary<string, string> GetMappings() {
            return biroToWooMap;
        }

        public Dictionary<string, WooAttr> GetAttributeMappingsFull()
        {
            return biroToWooAttributeMap;
        }

        public List<string> GetCategoryAttributes() {
            return categoryAttributes;
        }

        public BiroTaxToWooTax GetTax() {
            return this.tax;
        }

        public bool GetZaloga() {
            return includeZaloga;
        }

        public WooProductType GetProductType()
        {
            return wooProductType;
        }

        #region [add mappings]
        public BirokratArtikelToWooProductMapping AddMapping(string birokratProperty, string wooProperty)
        {
            biroToWooMap.Add(birokratProperty, wooProperty);
            return this;
        }
        public BirokratArtikelToWooProductMapping AddCategoryMapping(string birokratProperty)
        {

            categoryAttributes.Add(birokratProperty);

            return this;
        }
        public async Task<BirokratArtikelToWooProductMapping> AddAttributeMapping(string birokratProperty, WooAttr wooAttribute)
        {
            biroToWooAttributeMap.Add(birokratProperty, wooAttribute);
            if (attributeMapper == null)
                attributeMapper = new AttributeMapper(wooclient, wooProductType, biroToWooAttributeMap);
            await attributeMapper.CacheGlobalAttribute(wooAttribute);

            return this;
        }
        #endregion

        #region [executors]
        public async Task<Dictionary<string, object>> CompleteProductMapping(Dictionary<string, object> biroArtikel)
        {

            var woojson = new Dictionary<string, object>();

            if (wooProductType == WooProductType.SIMPLE)
            {
                woojson["type"] = "simple";
            }
            else if (wooProductType == WooProductType.VARIABLE)
            {
                woojson["type"] = "variable";
            }
            if (tax != null)
                woojson = tax.Map(woojson, biroArtikel);
            woojson = MapZaloga(woojson, biroArtikel);
            woojson = MapRegularProperties(woojson, biroArtikel);
            if (attributeMapper != null)
            {
                var arr = attributeMapper.MapAttributesAndEnsureGlobalAttributesAndTermsUploaded(biroArtikel);
                if (arr != null && arr.Length > 0)
                {
                    woojson["attributes"] = arr;
                }
            }
            woojson = MapCategories(woojson, biroArtikel);

            return woojson;
        }

        public async Task<Dictionary<string, object>> CompleteVariationMapping(Dictionary<string, object> biroArtikel,
                string parentId,
                ArtikelToProductMapping baseProductMapping)
        {

            if (parentId == "")
            {
                parentId = await UploadBaseProduct(biroArtikel, false, parentId, baseProductMapping);
            }
            else
            {
                EnsureNeededAttributesAreUploaded(biroArtikel, parentId, baseProductMapping);
            }

            var woojson = new Dictionary<string, object>();

            woojson = MapRegularProperties(woojson, biroArtikel);

            if (attributeMapper != null)
            {
                var arr = attributeMapper.MapAttributesVariation(biroArtikel);
                if (arr != null && arr.Length > 0)
                {
                    woojson["attributes"] = arr;
                }
            }
            if (includeZaloga)
            {
                woojson = MapZaloga(woojson, biroArtikel);
            }

            woojson["parent_id"] = parentId;

            return woojson;
        }

        private async Task<string> UploadBaseProduct(Dictionary<string, object> biroArtikel,
            bool privateProduct,
            string productId,
            ArtikelToProductMapping baseProductMapping)
        {
            try
            {
                var woot = await baseProductMapping.CompleteProductMapping(biroArtikel);
                if (privateProduct)
                {
                    woot["status"] = "private";
                }


                /*
                string woojson = JsonConvert.SerializeObject(woot);
                string result = wooclient.Post($"products", woojson);
                var tmp1 = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<Dictionary<string, object>>(result);
                var tmp = new { id = GWooOps.SerializeIntWooProperty(tmp1["id"]), type = (string)tmp1["type"] };
                if (string.IsNullOrEmpty(tmp.id)) throw new Exception("POST product call failed");
                productId = tmp.id;
                return productId;
                */

                woot["status"] = "draft";
                var result = wooclient.PostBaseVariableProduct(woot).GetAwaiter().GetResult();
                return GWooOps.SerializeIntWooProperty(result["id"]);






            }
            catch (Exception ex)
            {
                throw new ProductAddingException("Error during uploading base product", ex);
            }
        }
        #endregion

        #region [regular_props]
        private Dictionary<string, object> MapRegularProperties(Dictionary<string, object> woojson, Dictionary<string, object> biroArtikel)
        {
            foreach (var key in biroToWooMap.Keys)
            {
                woojson[biroToWooMap[key]] = biroArtikel[key];
            }
            return woojson;
        }
        private Dictionary<string, object> MapZaloga(Dictionary<string, object> woojson, Dictionary<string, object> biroArtikel)
        {
            string zaloga = (string)biroArtikel["zaloga"];
            if (string.IsNullOrEmpty(zaloga))
            {
                woojson["stock_quantity"] = "0";
                woojson["manage_stock"] = true;
            }
            else
            {
                woojson["stock_quantity"] = zaloga;
                woojson["manage_stock"] = true;
            }
            return woojson;
        }

        #endregion

        #region [categories]
        private Dictionary<string, object> MapCategories(Dictionary<string, object> woojson,
            Dictionary<string, object> biroArtikel)
        {


            /*
            // get woo categories
            string cats = wooclient.Get("products/categories?per_page=100");
            if (cats.Contains("\"code\":\"rest_invalid_param\""))
                cats = wooclient.Get("products/categories");
            List<dynamic> some = new List<dynamic>();
            var deser = JsonConvert.DeserializeAnonymousType(cats, new[] { new { id = "", name = "" } });
            */

            List<dynamic> some = new List<dynamic>();
            var deser = wooclient.GetCategories().GetAwaiter().GetResult();

            // foreach category

            foreach (string catkey in categoryAttributes)
            {
                string category = (string)biroArtikel[catkey];
                if (string.IsNullOrEmpty(category)) continue;
                string id = "";

                var sm = deser.Where(x => x.name.ToLower() == category.ToLower()).ToArray();

                // if category already added
                if (sm.Length > 0)
                {
                    id = sm.Single().id;
                    some.Add(new { id = id });
                }
                else
                { // if not added the add it yourself


                    /*
                    string json = wooclient.Post("products/categories", $@"{{""name"": ""{category}""}}");
                    var tp = JsonConvert.DeserializeAnonymousType(json, new { id = "" });
                    */

                    var tp = wooclient.PostCategory(category);





                    some.Add(tp);
                }
            }

            if (some.Count > 0)
                woojson["categories"] = some;
            return woojson;
        }
        #endregion

        #region [attributes]
        public dynamic[] MapAttributesAndEnsureGlobalAttributesAndTermsUploaded(Dictionary<string, object> biroArtikel)
        {
            return attributeMapper.MapAttributesAndEnsureGlobalAttributesAndTermsUploaded(biroArtikel);
        }

        public void AppendAttrTermsToProductAttributeDomain(Dictionary<string, object> biroArtikel, string productId)
        {


            /*
            string prod = wooclient.Get($"products/{productId}");
            var obj = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<TmpCls> (prod).Attributes;
            */
            var productAttributeDomain = wooclient.GetAttributes(productId).GetAwaiter().GetResult();








            // add current terms in
            bool changeHappened = false;
            foreach (var biroAttributeName in biroToWooAttributeMap.Keys)
            {
                string currBiroAttributeValue = (string)biroArtikel[biroAttributeName];
                if (!string.IsNullOrWhiteSpace(currBiroAttributeValue))
                {
                    //value = TextUtils.RemoveSumniks(value);
                    WooAttr wooattribute = biroToWooAttributeMap[biroAttributeName];
                    string wooAttributeId = attributeMapper.attributeNameToIdMap[wooattribute.Name];
                    foreach (var wooAttribute in productAttributeDomain)
                    {
                        if (wooAttribute["id"].ToString() == wooAttributeId)
                        { // found the right attribute
                            JArray opts1 = (JArray)wooAttribute["options"];
                            List<string> opts = opts1.ToObject<List<string>>();
                            if (!opts.Select(x => x.ToLower()).Contains(currBiroAttributeValue.ToLower()))
                            { // actual change happened
                                opts.Add(currBiroAttributeValue.ToLower());
                                wooAttribute["options"] = opts;
                                changeHappened = true;
                            }
                            break;
                        }
                    }
                }
            }

            if (changeHappened)
            {

                /*
                string body = JsonConvert.SerializeObject(new { attributes = obj });
                string res = wooclient.Put($"products/{productId}", body);
                */

                var wooobj = new Dictionary<string, object>() { { "attributes", productAttributeDomain } };
                wooclient.UpdateProduct(productId, wooobj);

            }
        }

        private void EnsureNeededAttributesAreUploaded(Dictionary<string, object> biroArtikel, string productId, ArtikelToProductMapping baseProductMapping)
        {
            try
            {
                var woot = new Dictionary<string, object>();
                var arr = baseProductMapping.MapAttributesAndEnsureGlobalAttributesAndTermsUploaded(biroArtikel);
                if (arr != null && arr.Length > 0)
                {
                    woot["attributes"] = arr;
                }

                baseProductMapping.AppendAttrTermsToProductAttributeDomain(biroArtikel, productId); // add new attributes if necessary
            }
            catch (Exception ex)
            {
                throw new ProductAddingException("Error during ensuring needed attributes are uploaded", ex);
            }
        }
        #endregion

    }
}