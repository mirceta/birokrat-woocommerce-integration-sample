using BirokratNext;
using ShopifySharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webshop_client_shopify {

    public class ShopifyProductBridge
    {
        string storeUrl;
        string access_token;
        Dictionary<string, ProductVariant> skuToProductMap;
        IApiClientV2 client;

        public ShopifyProductBridge(string storeUrl,
            string access_token,
            Dictionary<string, ProductVariant> skuToProductMap,
            IApiClientV2 client) {
            this.storeUrl = storeUrl;
            this.access_token = access_token;
            this.skuToProductMap = skuToProductMap;
            this.client = client;
        }


        #region [shop_accessors]
        public async Task<Dictionary<string, object>> UpdateProduct(string sku, Dictionary<string, object> values) {

            var variant = skuToProductMap[sku];

            var service = new ProductService(storeUrl, access_token);
            var product = await service.GetAsync((long)variant.ProductId);

            if (values.Keys.Contains("regular_price")) {
                await UpdatePrice(product, sku, (string)values["regular_price"]);
            }

            if (values.Keys.Contains("stock_quantity")) {
                await new ShopifyStockUpdater(storeUrl, access_token).UpdateStock(product, sku, int.Parse((string)values["stock_quantity"]));
            }

            return ToWooVariation(product, variant);
        }

        public async Task<Dictionary<string, object>> PostSimpleProduct(Dictionary<string, object> wooobj) {
            var product = new Product();

            product.Title = (string)wooobj["name"];
            if (wooobj.ContainsKey("description"))
                product.BodyHtml = (string)wooobj["description"];

            ProductVariant variant = new ProductVariant();

            variant.TaxCode = (string)wooobj["tax_class"]; //!!!!!!!!!!!!!!!!!!!!
            variant.SKU = (string)wooobj["sku"];


            var price = (decimal)double.Parse((string)wooobj["regular_price"]); //!!!!!!!!!!!!!!!!!!!!  
            variant.CompareAtPrice = price;
            variant.Price = price; // PRI NOVEM IZDELKU POPRAVIMO TUDI PRICE - CE NE OSTANE 0 IN JE 100% POPUST!!


            product.Variants = new List<ProductVariant>() { variant };

            // attributes!

            // categories!

            var service = new ProductService(storeUrl, access_token);
            product = await service.CreateAsync(product);

            // manage stock on!
            await new ShopifyStockUpdater(storeUrl, access_token).UpdateStock(product, variant.SKU, int.Parse((string)wooobj["stock_quantity"]));

            product = await service.GetAsync((long)product.Id);
            return ToWooVariation(product, variant); // ???????
        }

        public async Task<Dictionary<string, object>> PostVariation(Dictionary<string, object> wooobj) {

            return await new ShopifyVariableProductCreator(storeUrl, access_token, client, skuToProductMap).PostVariation(wooobj);

        }
        #endregion

        #region [shopify_to_integ_adapter]
        public Dictionary<string, object> ToWooVariation(Product product, ProductVariant variant) {
            return new ProductAdapter().ShopifyToWoo(product, variant);
        }
        #endregion

        /*
         FROM HERE ON PRIVATE
         */

        #region [auxiliary]
       
        async Task UpdatePrice(Product product, string sku, string price) {

            var variants = product.Variants.Where(x => x.SKU == sku).ToList();
            if (variants.Count == 0)
                throw new Exception($"Cannot update price of unknown sku {sku}");
            if (variants.Count > 1)
                throw new Exception($"Multiple variants have the same sku {sku}");

            // change price
            var service = new ProductService(storeUrl, access_token);
            var varian = variants.Single();
            varian.CompareAtPrice = (decimal?)ParseDoubleBigBrainTime(price);

            product = await service.UpdateAsync((long)product.Id, product);
        }

        #endregion

        #region [auxiliary]
        public static double ParseDoubleBigBrainTime(string number) {
            int cnt = number.Where(x => x == ',' || x == '.').ToList().Count;

            if (string.IsNullOrEmpty(number)) {
                return 0;
            } else if (cnt == 0) {
                return double.Parse(number);
            } else if (cnt == 1) {
                string some = number.Replace(".", ",");
                CultureInfo culture = new CultureInfo("de"); // de culture means '.' is thousands sep, ',' is decimal sep
                return double.Parse(some, culture);
            } else {
                char decimalsep = number.Where(x => x == ',' || x == '.').Last();

                if (decimalsep == ',') {
                    number = number.Replace(".", "");
                } else {
                    number = number.Replace(",", "");
                }
                number = number.Replace(".", ",");
                CultureInfo culture = new CultureInfo("de"); // de culture means '.' is thousands sep, ',' is decimal sep
                return double.Parse(number, culture);
            }
        }
        #endregion
    }
}
