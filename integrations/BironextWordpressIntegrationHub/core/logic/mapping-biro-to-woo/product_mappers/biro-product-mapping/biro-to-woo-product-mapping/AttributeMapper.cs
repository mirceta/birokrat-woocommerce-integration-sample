using birowoo_exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWoocommerceHubTests.tools {
    class AttributeMapper {

        IOutApiClient wooclient;
        WooProductType wooProductType;
        Dictionary<string, WooAttr> biroToWooAttributeMap;
        public Dictionary<string, string> attributeNameToIdMap;
        public AttributeMapper(IOutApiClient wooclient,
            WooProductType type,
            Dictionary<string, WooAttr> biroToWooAttributeMap) {
            this.wooclient = wooclient;
            wooProductType = type;
            this.biroToWooAttributeMap = biroToWooAttributeMap;

            attributeNameToIdMap = new Dictionary<string, string>();
        }

        public async Task CacheGlobalAttribute(WooAttr wooAttribute) {
            FetchAllWooAttributesAndCacheThem();
            int attrid = FindCurrentAttribute(wooAttribute);
            if (attrid == -1) {
                attrid = await PostNewAttributeAndReturnId(wooAttribute.Name);
            }
            attributeNameToIdMap.Add(wooAttribute.Name, "" + attrid);
        }

        public dynamic[] MapAttributesAndEnsureGlobalAttributesAndTermsUploaded(Dictionary<string, object> biroArtikel) {
            Func<string, int, bool, WooAttr, string, string> jsonTemplate = (id, i, variation, wooattribute, term) => $@"{{
                    ""id"":{id},
                    ""position"":{i},
                    ""visible"": {wooattribute.Visible.ToString().ToLower()},
                    ""variation"": {variation.ToString().ToLower()},
                    ""name"":""{wooattribute.Name}"",
                    ""options"": [""{term}""]
                }}";
            var some = new { id = "", position = 0, visible = true, variation = true, name = "", options = new[] { "" } };
            var arr = MapAttributesAndEnsureUploaded(jsonTemplate, some, biroArtikel);

            return arr;
        }
        public dynamic[] MapAttributesVariation(Dictionary<string, object> biroArtikel) {
            Func<string, int, bool, WooAttr, string, string> jsonTemplate = (id, i, variation, wooattribute, term) => $@"{{
                    ""id"":{id},
                    ""option"": ""{term}""
                }}";
            var some = new { id = "", option = "" };
            var arr = MapAttributesAndEnsureUploaded(jsonTemplate, some, biroArtikel);
            return arr;
        }

        public dynamic[] MapAttributesAndEnsureUploaded(Func<string, int, bool, WooAttr, string, string> template,
            dynamic objTemplate,
            Dictionary<string, object> biroArtikel) {
            var cache = new WooAttributeTermCache(wooclient);
            var wooattrs = new Dictionary<string, string>();
            bool variation = wooProductType == WooProductType.VARIABLE;

            int i = 0;
            foreach (var key in biroToWooAttributeMap.Keys) {
                string value = (string)biroArtikel[key];
                if (!string.IsNullOrWhiteSpace(value)) {
                    //value = TextUtils.RemoveSumniks(value);
                    WooAttr wooattribute = biroToWooAttributeMap[key];
                    string id = attributeNameToIdMap[wooattribute.Name];
                    string term = cache.GetOrAddTerm(id, value);
                    string val = template(id, i++, variation, wooattribute, term);
                    if (wooattribute.Mandatory && string.IsNullOrEmpty(term)) {
                        throw new IntegrationProcessingException($"Article did not contain the value of attribute {wooattribute.Name}");
                    }
                    wooattrs[wooattribute.Name] = val;
                }
            }

            dynamic[] arr = null;
            if (wooattrs.Count > 0) {
                arr = wooattrs.Select(x =>
                    JsonConvert.DeserializeAnonymousType(x.Value, objTemplate))
                    .ToArray();
            }
            return arr;
        }

        dynamic attributeCache = null;
        private void FetchAllWooAttributesAndCacheThem() {
            var anon0 = new { id = "", name = "", slug = "" };
            if (attributeCache == null) {


                /*
                string res = wooclient.Get("products/attributes");
                var anon = new[] { anon0 };
                attributeCache = JsonConvert.DeserializeAnonymousType(res, anon);
                */

                var lst = wooclient.GetAttributes().GetAwaiter().GetResult();
                var anon = new[] { anon0 };
                attributeCache = JsonConvert.DeserializeAnonymousType(JsonConvert.SerializeObject(lst), anon);





            }
        }

        private int FindCurrentAttribute(WooAttr wooAttribute) {
            int attrid = -1;
            foreach (var x in attributeCache) {
                if (x.slug.Replace("pa_", "") == wooAttribute.Name.ToLower())
                    attrid = int.Parse(x.id);
            }
            return attrid;
        }

        private async Task<int> PostNewAttributeAndReturnId(string wooAttribute) {
            var anon0 = new { id = "", name = "" };
            string attributesBody = $@"
                {{
                    ""name"": ""{wooAttribute}"",
                    ""slug"": ""pa_{wooAttribute.ToLower()}"",
                    ""type"": ""select"",
                    ""order_by"": ""menu_order"",
                    ""has_archives"": true
                }}
                ";


            /*
            string res = wooclient.Post("products/attributes", attributesBody);
            */
            var tmp = JsonConvert.DeserializeObject<Dictionary<string, object>>(attributesBody);
            tmp = await wooclient.PostAttribute(tmp);
            string res = JsonConvert.SerializeObject(tmp);



            try {
                int attrid = int.Parse(JsonConvert.DeserializeAnonymousType(res, anon0).id);
                return attrid;
            } catch (Exception ex) {
                throw new Exception(res, ex);
            }

        }
    }
}