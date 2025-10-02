using ShopifySharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace webshop_client_shopify {
    public class ShopifyStockUpdater {

        string storeUrl;
        string access_token;
        public ShopifyStockUpdater(string storeUrl, string access_token) {
            this.storeUrl = storeUrl;
            this.access_token = access_token;
        }

        public async Task UpdateStock(Product product, string sku, int stock_quantity) {


            var variants = product.Variants.Where(x => x.SKU == sku).ToList();
            if (variants.Count == 0)
                throw new Exception($"Cannot update stock of unknown sku {sku}");
            if (variants.Count > 1)
                throw new Exception($"Multiple variants have the same sku {sku}");

            var variantInventoryId = variants.Single().InventoryItemId;

            var locationService = new LocationService(storeUrl, access_token);
            var kurac = await locationService.ListAsync();


            var inventoryService = new InventoryItemService(storeUrl, access_token);
            var invnet = await inventoryService.GetAsync((long)variantInventoryId);
            invnet.Tracked = true;
            var some = await inventoryService.UpdateAsync((long)variantInventoryId, invnet);

            var inventoryLevelService = new InventoryLevelService(storeUrl, access_token);
            var tmpa = await inventoryLevelService.SetAsync(new InventoryLevel {
                InventoryItemId = (long)variantInventoryId,
                LocationId = kurac.ToList()[0].Id,
                Available = stock_quantity
            });

            if (tmpa.Available != (long)stock_quantity) {
                throw new Exception("Stock not updated even though shopify API said that it will get!");
            }

            var service = new ProductService(storeUrl, access_token);
            product = await service.GetAsync((long)product.Id);
        }
    }
}
