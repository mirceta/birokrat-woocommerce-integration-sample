using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace tests_webshop.products
{
    public interface IProductTransferAccessor {
        Task AddOrUpdate(ProductTransfer pt);

        Task<List<ProductTransfer>> List();

        void Delete(string productid);
    }
}
