using core.structs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_biro_to_woo
{
    public interface IBiroToWooProductSyncer
    {
        Task UpdateProduct(Dictionary<string, object> biroArtikel, bool privateProduct = false);
        Task AddProduct(Dictionary<string, object> biroArtikel, bool privateProduct = false);
    }
}
