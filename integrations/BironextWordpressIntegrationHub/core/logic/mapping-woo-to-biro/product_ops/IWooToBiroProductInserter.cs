using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.product_ops {
    public interface IWooToBiroProductInserter {
        Task OnArticleAddedRaw(string product_id, string variation_id);
        Task OnArticleChangedRaw(string product_id, string variation_id);
    }
}