using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests
{
    public interface WebshopProductDeleter {
        Task<DeletionResult> DeleteProductBySku(string sku);
        Task DeleteProductById(string id);
        Task DeleteProducts(List<Dictionary<string, object>> products);
        Task DeleteAllProducts();
    }
}
