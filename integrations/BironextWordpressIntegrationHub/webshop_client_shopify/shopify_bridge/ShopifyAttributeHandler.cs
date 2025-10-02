using birowoo_exceptions;
using Newtonsoft.Json;
using ShopifySharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace webshop_client_shopify
{
    class ShopifyAttributeHandler {

        /*
         Inner workings:
            When adding a new variant to a product with existing variants.
            New product has an attribute that the others do not.
            For the new attribute, we will set the default value in all other variants.
         */

        public ShopifyAttributeHandler() { 
        
        }

        public void HandleAttributes(Dictionary<string, object> wooobj, Product product, ProductVariant variant) {
            var x = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(wooobj["attributes"]));
            foreach (var attr in x) {
                string attrName = (string)attr.Key;
                string attrValue = (string)attr.Value;

                HandleAttribute(product, variant, attrName, attrValue);
            }
        }

        public string GetOptionValue(int opt, ProductVariant varia) {
            switch (opt) {
                case 1:
                    return varia.Option1;
                case 2:
                    return varia.Option2;
                case 3:
                    return varia.Option3;
            }
            throw new IntegrationProcessingException($"There can be no other option apart from 1 2 or 3. But {opt} selected.");
        }

        private void HandleAttribute(Product product, ProductVariant variant, string attrName, string attrValue) {
            var optionOfAttribute = product.Options.Where(x => x.Name == attrName).ToList();
            if (optionOfAttribute.Count == 0) { // attr not exists 
                product = new NewProductAttributeHandler(product, variant, attrName, attrValue).UpdateProduct();
            } else {
                variant = AddOldAttributeToNewVariant(variant, attrValue, optionOfAttribute);
            }
        }

        private ProductVariant AddOldAttributeToNewVariant(ProductVariant variant, string attrValue, List<ProductOption> optionOfAttribute) {
            var opt = optionOfAttribute.Single();
            int pos = (int)opt.Position;
            var tmp = opt.Values.ToList();
            tmp.Add(attrValue);
            opt.Values = tmp;

            variant = ShopifyAttributeHelper.MapOptionValue(pos, variant, attrValue);
            return variant;
        }

    }

    class NewProductAttributeHandler {

        Product product;
        ProductVariant variant;
        string attrName;
        string attrValue;
        public NewProductAttributeHandler(Product product, ProductVariant newVariant, string attrName, string attrValue) {
            this.product = product;
            this.variant = newVariant;
            this.attrName = attrName;
            this.attrValue = attrValue;
        }

        public Product UpdateProduct() {
            if (product.Options.Count() >= 3)
                throw new IntegrationProcessingException($"Attempted to add another option {attrName} to Product, but existing variants already contain 3 variation attributes. More than 3 variation attributes are not permitted on shopify");

            var option = UpdateProductOptionDomain();
            
            return UpdateVariantsWithOptionValue((int)option.Position);
        }

        

        #region [update option domain]
        private ProductOption UpdateProductOptionDomain() {
            ProductOption opt;
            var options = product.Options.ToList();
            if (product.Options.Any(x => x.Name == "Title")) {
                opt = ReplaceDefaultTitleOptionWithNewOption();
            } else {
                opt = AddNewOptionToOptionDomain(options);
            }
            product.Options = options;
            return opt;
        }

        private ProductOption AddNewOptionToOptionDomain(List<ProductOption> options) {
            ProductOption opt = new ProductOption {
                Name = attrName,
                Values = new string[] { "Default", attrValue },
                Position = product.Options.Count() + 1
            };
            options.Add(opt);
            return opt;
        }

        private ProductOption ReplaceDefaultTitleOptionWithNewOption() {
            ProductOption opt = product.Options.Where(x => x.Name == "Title").Single();
            opt.Name = attrName;
            opt.Values = new string[] { "Default", attrValue };
            return opt;
        }
        #endregion

        #region [update values in variations]
        private Product UpdateVariantsWithOptionValue(int newOptionPosition) {
            var variancias = product.Variants.ToList();
            for (int i = 0; i < variancias.Count; i++) {
                if (variancias[i].SKU == variant.SKU) {
                    variancias[i] = ShopifyAttributeHelper.MapOptionValue(newOptionPosition, variant, attrValue);
                } else {
                    variancias[i] = ShopifyAttributeHelper.MapOptionValue(newOptionPosition, variancias[i], "Default");
                }
            }
            product.Variants = variancias;
            return product;
        }
        #endregion
    }

    class ShopifyAttributeHelper {
        public static ProductVariant MapOptionValue(int opt, ProductVariant varia, string attrValue) {
            switch (opt) {
                case 1:
                    varia.Option1 = attrValue;
                    break;
                case 2:
                    varia.Option2 = attrValue;
                    break;
                case 3:
                    varia.Option3 = attrValue;
                    break;
            }
            return varia;
        }
    }
}
