using BirokratNext;
using birowoo_exceptions;
using BiroWoocommerceHubTests;
using Newtonsoft.Json;
using ShopifySharp;
using ShopifySharp.Filters;
using ShopifySharp.Lists;
using si.birokrat.next.common.build;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using validator;

namespace webshop_client_shopify
{
    public class ShopifyApiClient : IOutApiClient
    {

        string storeUrl;
        string access_token;
        IApiClientV2 client;

        public ShopifyApiClient(string storeUrl, string access_token, IApiClientV2 client) {
            this.storeUrl = storeUrl;
            this.access_token = access_token;
            GetProducts().GetAwaiter().GetResult();
            this.client = client;
        }

        public string Ck => throw new NotImplementedException();

        public string Cs => throw new NotImplementedException();

        public string Address => throw new NotImplementedException();


        Dictionary<string, ProductVariant> skuToProductMap;
        private async Task ParseSkuToProductMap(List<Product> prods) {
            skuToProductMap = new Dictionary<string, ProductVariant>();
            foreach (var prod in prods) {
                foreach (var varia in prod.Variants) {
                    if (!string.IsNullOrEmpty(varia.SKU)) {
                        skuToProductMap[varia.SKU] = varia;
                    }
                }
            }
        }

        #region [product]
        public async Task<List<Dictionary<string, object>>> GetProducts() {

            var prods = await GetAllProducts();

            await ParseSkuToProductMap(prods); // SIDE EFFECT - ALSO REFRESH THE LOCAL CACHE OF PRODUCTS!!!

            var bridge = new ShopifyProductBridge(storeUrl, access_token, skuToProductMap, client);
            var res = prods.Select(x => x.Variants.Select(v => new Tuple<Product, ProductVariant>(x, v)).ToList()).ToList();

            var kurac = res.Where(x => x.Count == 5).ToList();

            var tmp1 = res.Aggregate(new List<Tuple<Product, ProductVariant>>(), (x, y) => { x.AddRange(y); return x; }).ToList();

            var tmp2 = tmp1.Select(x => bridge.ToWooVariation(x.Item1, x.Item2))
                        .ToList();
            return tmp2;
        }

        public async Task<ProductResult> GetProductBySku(string sku) {

            if (!skuToProductMap.ContainsKey(sku))
            {
                return ProductResult.FailureResult("The product was not found");
            }

            long id = 0; ;
            if (skuToProductMap.ContainsKey(sku)) {
                var some = skuToProductMap[sku];
                id = (long)some.ProductId;
            } else {
                throw new ProductNotFoundException($"Product with sku {sku} not found");
            }

            

            var service = new ProductService(storeUrl, access_token);
            var chome = await service.GetAsync(id);
            var variant = chome.Variants.Where(x => x.SKU == sku).Single();
            var tmp = new ShopifyProductBridge(storeUrl, access_token, skuToProductMap, client)
                        .ToWooVariation(chome, variant);
            return ProductResult.SuccessResult(tmp);
        }
        public Task<string> DeleteProduct(string id) {
            throw new NotImplementedException();
        }
        public Task<string> DeleteProductBySku(string sku) {
            throw new NotImplementedException();
        }







        public async Task<Dictionary<string, object>> PostProduct(Dictionary<string, object> product) {

            try {
                if (product.ContainsKey("variant")) {
                    return await new ShopifyProductBridge(storeUrl, access_token, skuToProductMap, client).PostVariation(product);
                } else {
                    return await new ShopifyProductBridge(storeUrl, access_token, skuToProductMap, client).PostSimpleProduct(product);
                }
            } catch (Exception ex) {
                if (product.ContainsKey("sku")) {
                    throw new IntegrationProcessingException($"Error during posting product {(string)product["sku"]}: {ex.Message}");
                }
                throw new IntegrationProcessingException($"SKU code was empty during PostProduct!");
            }
        }

        public async Task<Dictionary<string, object>> UpdateProduct(string sku, Dictionary<string, object> values) {
            return await new ShopifyProductBridge(storeUrl, access_token, skuToProductMap, client).UpdateProduct(sku, values);
        }

        #endregion

        #region [not to be implemented]


        public Task<string> DeleteVariation(string parent_id, string variation_id) {
            throw new NotImplementedException();
        }
        public Task<List<Dictionary<string, object>>> GetVariableProducts() {
            throw new NotImplementedException();
        }
        public async Task<Dictionary<string, object>> PostBaseVariableProduct(Dictionary<string, object> product) {
            throw new NotImplementedException();
        }
        public Task<Dictionary<string, object>> UpdateVariation(string product_id, string variation_id, Dictionary<string, object> values) {
            throw new NotImplementedException();
        }
        public async Task<Dictionary<string, object>> PostVariation(string parent_id, Dictionary<string, object> variation) {
            throw new NotImplementedException();
        }
        #region [attributes]

        public async Task<List<Dictionary<string, object>>> GetAttributes() {
            throw new NotImplementedException();
        }

        public async Task<List<Dictionary<string, object>>> GetAttributes(string productId) {
            throw new NotImplementedException();
        }

        public async Task<List<Dictionary<string, object>>> GetAttributeTerms(string attributeId) {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, object>> PostAttribute(Dictionary<string, object> attribute) {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, object>> PostAttributeTerm(string attributeId, string attributeTerm) {
            throw new NotImplementedException();
        }
        #endregion


        #region [category]
        public async Task<Category> PostCategory(string name) {
            return new Category();
        }

        public async Task<List<Category>> GetCategories() {
            return new List<Category>();
        }
        #endregion
        #endregion


        #region [other]
        public async Task<string> MyGetOrder(string id) {
            var service = new OrderService(storeUrl, access_token);

            var zum = await service.ListAsync();
            var some = zum.Items.ToList()[0];

            var okasdr = JsonConvert.SerializeObject(some);

            var woo = new ShopifyToWooOrderAdapter().Adapt(some);
            okasdr = JsonConvert.SerializeObject(woo);

            return okasdr;
        }

        public Task<string> DeleteOrderTransfer(string orderid, string orderstatus) {
            throw new NotImplementedException();
        }
        public Task<string> GetOrderTransfer(string orderTransfer) {
            throw new NotImplementedException();
        }

        public Task<string> PutKita(string query, string body) {
            throw new NotImplementedException();
        }

        public Task<string> AddUnacceptedOrderTransfer(string orderId, string orderStatus) {
            throw new NotImplementedException();
        }

        public Task<string> GetKita(string query) {
            throw new NotImplementedException();
        }

        public Task<string> GetOrderTransfers() {
            throw new NotImplementedException();
        }
        #endregion

        private async Task<List<Product>> GetAllProducts() {
            var products = new List<Product>();
            var service = new ProductService(storeUrl, access_token);

            long? lastId = 0;
            while (lastId >= 0) {
                var filter = new ProductListFilter {
                    SinceId = lastId
                };

                var productList = await service.ListAsync(filter);
                if (productList != null && productList.Items.Any()) {
                    products.AddRange(productList.Items);
                    lastId = productList.Items.Last().Id;
                } else {
                    break;
                }
            }

            if (await service.CountAsync() != products.Count)
                throw new Exception("Shopify API did not return all products!");

            return products;
        }

        
        public void SetLogger(IMyLogger logger) { 
        
        }
        public Task<string> Get(string msg) {
            throw new NotImplementedException();
        }

        public Task DeleteProductTransfer(string productid)
        {
            throw new NotImplementedException();
        }

        public Task<string> PostKita(string query, string body)
        {
            throw new NotImplementedException();
        }

        public Task<string> DeleteKita(string query)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetOrders(DateTime sinceDate)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetOrderDescriptions(DateTime sinceDate)
        {
            throw new NotImplementedException();
        }

        public Task<List<List<StatusChange>>> GetOrderStatusChanges(List<int> orderIds)
        {
            throw new NotImplementedException();
        }
    }
}
