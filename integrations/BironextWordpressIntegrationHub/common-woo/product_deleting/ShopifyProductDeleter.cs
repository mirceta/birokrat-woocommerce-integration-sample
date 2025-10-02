using BiroWoocommerceHubTests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using tests;

namespace common_woo.product_deleting
{
    public class ShopifyProductDeleter : WebshopProductDeleter
    {

        IOutApiClient client;

        public ShopifyProductDeleter(IOutApiClient client) {
            this.client = client;
        }

        public Task DeleteAllProducts() {
            throw new NotImplementedException();
        }

        public Task DeleteProductById(string id) {
            throw new NotImplementedException();
        }

        public Task<DeletionResult> DeleteProductBySku(string sku) {
            throw new NotImplementedException();
        }

        public Task DeleteProducts(List<Dictionary<string, object>> products) {
            throw new NotImplementedException();
        }
    }
}
