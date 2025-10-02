using BirokratNext;
using birowoo_exceptions;
using core.logic.common_birokrat;
using core.tools.birokratops;
using ShopifySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webshop_client_shopify {
    public class ShopifyVariableProductCreator {

        IApiClientV2 client;
        Dictionary<string, ProductVariant> skuToProductMap;
        string storeUrl;
        string access_token;
        public ShopifyVariableProductCreator(string storeUrl, string access_token, IApiClientV2 client, Dictionary<string, ProductVariant> skuToProductMap) {
            this.client = client;
            this.skuToProductMap = skuToProductMap;
            this.storeUrl = storeUrl;
            this.access_token = access_token;
        }

        public async Task<Dictionary<string, object>> PostVariation(Dictionary<string, object> wooobj) {

            var service = new ProductService(storeUrl, access_token);

            string variantSku = await FindUploadedVariation_WithSameCommonSku(wooobj);
            Console.WriteLine(variantSku);
            if (string.IsNullOrEmpty(variantSku) || !skuToProductMap.ContainsKey(variantSku)) {
                Console.WriteLine("UPLOAD NEW");
                return await UploadVariationToNewProduct(wooobj, service);
            } else {
                Console.WriteLine("UPLOAD OLD");
                return await UploadVariationToExistingProduct(wooobj, service, variantSku);
            }
        }

        private async Task<string> FindUploadedVariation_WithSameCommonSku(Dictionary<string, object> wooobj) {
            string commonSku = ((string)wooobj["variant"]).Trim();
            var result = await new PodrobniPregledArtiklov().GetPodrobniPregledArtiklov(client);
            result = result.Where(x => x.ContainsKey("Barkoda 4") && ((string)x["Barkoda 4"]).Trim() == commonSku.Trim()).ToList();

            string variantSku = "";

            string okasd = "";
            foreach (var x in result) {
                if (skuToProductMap.ContainsKey((string)x["Artikel"])) {
                    okasd = (string)x["Artikel"];
                    variantSku = okasd;
                }
            }

            return variantSku;
        }

        private async Task<Dictionary<string, object>> UploadVariationToExistingProduct(Dictionary<string, object> wooobj, ProductService service, string variantSku) {
            var pr = skuToProductMap[variantSku];
            long id = (long)pr.ProductId;
            var product = await service.GetAsync(id);

            string sku = (string)wooobj["sku"];
            if (product.Variants.Any(x => x.SKU == sku)) {
                throw new IntegrationProcessingException("Product already added");
            }
            return await CreateVariation(service, wooobj, product, sku);
        }

        private async Task<Dictionary<string, object>> UploadVariationToNewProduct(Dictionary<string, object> wooobj, ProductService service) {
            var product = await PostVariationRoot(wooobj);
            string sku = product.Variants.Single().SKU;
            product.Variants = new List<ProductVariant>();
            return await CreateVariation(service, wooobj, product, sku);
        }

        private async Task<Product> PostVariationRoot(Dictionary<string, object> wooobj) {

            var service = new ProductService(storeUrl, access_token);

            var product = new Product();

            product.Title = (string)wooobj["name"];
            product.Status = "draft";
            ProductVariant variant = new ProductVariant();

            variant.TaxCode = (string)wooobj["tax_class"]; //!!!!!!!!!!!!!!!!!!!!
            variant.SKU = (string)wooobj["sku"];
            variant.Price = (decimal)double.Parse((string)wooobj["regular_price"]); // PRI NOVEM IZDELKU POPRAVIMO TUDI PRICE KER CE NE 100% POPUST
            variant.CompareAtPrice = (decimal)double.Parse((string)wooobj["regular_price"]); //!!!!!!!!!!!!!!!!!!!!

            product.Variants = new List<ProductVariant>() { variant };

            product = await service.CreateAsync(product);

            // NO STOCK FOR ROOT VARIATION - PERHAPS WILL NEED TO HIDE THIS VARIATION???

            // UPDATE CACHE
            skuToProductMap[variant.SKU] = product.Variants.Single();

            return product;
        }

        private async Task<Dictionary<string, object>> CreateVariation(ProductService service, Dictionary<string, object> wooobj, Product product, string sku) {
            // CREATE VARIANT
            ProductVariant variant = new ProductVariant();


            // taxclass is added in root 
            variant.SKU = (string)wooobj["sku"];

            var price = (decimal)double.Parse((string)wooobj["regular_price"]); //!!!!!!!!!!!!!!!!!!!!  
            variant.CompareAtPrice = price;
            variant.Price = price; // PRI NOVEM IZDELKU POPRAVIMO TUDI PRICE - CE NE OSTANE 0 IN JE 100% POPUST!!
            var allvars = product.Variants.ToList();
            allvars.Add(variant);
            product.Variants = allvars;


            // handle attributes!
            new ShopifyAttributeHandler().HandleAttributes(wooobj, product, variant);

            product = await service.UpdateAsync((long)product.Id, product);

            // update stock!
            await new ShopifyStockUpdater(storeUrl, access_token).UpdateStock(product, sku, int.Parse((string)wooobj["stock_quantity"]));

            product = await service.GetAsync((long)product.Id);

            skuToProductMap[variant.SKU] = product.Variants.Where(x => x.SKU == sku).Single();

            return new ProductAdapter().ShopifyToWoo(product, variant);
        }
    }
}
