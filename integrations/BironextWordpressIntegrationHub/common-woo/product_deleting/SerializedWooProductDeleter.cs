using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests
{
    public class SerializedWooProductDeleter : WebshopProductDeleter
    {
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
