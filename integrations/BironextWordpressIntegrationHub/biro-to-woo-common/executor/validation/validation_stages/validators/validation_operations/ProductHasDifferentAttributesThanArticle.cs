using birowoo_exceptions;
using core.structs;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace biro_to_woo_common.executor.validation_stages.validators.validation_operations
{
    public class ProductHasDifferentAttributesThanArticle : IProductTransferVerifyOperation
    {

        string skuField;
        Dictionary<string, string> allPossibleAdditionAttrBiroToOut;

        public ProductHasDifferentAttributesThanArticle(string skuField, Dictionary<string, string> allPossibleAdditionAttrBiroToOut)
        {
            this.skuField = skuField;
            this.allPossibleAdditionAttrBiroToOut = allPossibleAdditionAttrBiroToOut;
        }

        public void Verify(string sifra, BiroOutComparisonContext context)
        {

            ProductTransferVerifyOperationHelper.nullGuards(sifra, context);

            var outItem = context.outItems.FirstOrDefault(element => element["sku"] as string == sifra);
            var biroItem = context.biroItems.FirstOrDefault(x => x[skuField] as string == sifra);

            if (outItem == null || biroItem == null)
            {
                throw new KeyNotFoundException($"Could not find the item with SKU {sifra}.");
            }

            if (outItem.ContainsKey("type") && (string)outItem["type"] == "variable" && GWooOps.SerializeIntWooProperty(outItem["parent_id"]) == "0") {
                // this is a root variation product
                return;
            }
            if (outItem.ContainsKey("type") && (string)outItem["type"] == "simple")
            {
                // we don't care about simple products
                return;
            }


            var anon = new[] { new { name = "", option = "" } };
            var attributesJson = outItem.ContainsKey("attributes") ? outItem["attributes"] : "[]";
            var tmp = JsonConvert.DeserializeAnonymousType(JsonConvert.SerializeObject(attributesJson), anon);
            Dictionary<string, string> wooAttrs = tmp.ToDictionary(x => x.name, x => x.option);

            foreach (var attr in allPossibleAdditionAttrBiroToOut)
            {
                string biroAttrName = attr.Key;
                string wooAttrName = attr.Value;

                wooAttrs.TryGetValue(wooAttrName, out string wooAttrValue);
                if (wooAttrValue == null) wooAttrValue = "";
                string biroAttrValue = biroItem.ContainsKey(biroAttrName) ? biroItem[biroAttrName] as string : "";
                if (biroAttrValue == null) biroAttrValue = "";

                if (wooAttrValue != biroAttrValue)
                    throw new IntegrationProcessingException($"SKU: {outItem["sku"]} Atribut {wooAttrName} ima drugačno vrednost na spletni trgovini ({wooAttrValue}) in v birokratu ({biroAttrValue})");
            }
        }
    }
}
