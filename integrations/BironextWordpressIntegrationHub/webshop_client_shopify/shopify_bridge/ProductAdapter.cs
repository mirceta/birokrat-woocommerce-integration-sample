using ShopifySharp;
using System.Collections.Generic;

namespace webshop_client_shopify
{
    class ProductAdapter {
        public ProductAdapter() { 
        
        }

        public Dictionary<string, object> ShopifyToWoo(Product product, ProductVariant variant) {
            Dictionary<string, object> wooobj = new Dictionary<string, object>();

            wooobj["id"] = product.Id;
            wooobj["name"] = product.Title;
            wooobj["description"] = product.BodyHtml;


            wooobj["sku"] = variant.SKU;

            string price = variant.CompareAtPrice.ToString();
            string sale_price = variant.Price.ToString();

            wooobj["regular_price"] = price;
            //wooobj["price"] = price;
            wooobj["sale_price"] = sale_price;

            wooobj["parent_id"] = product.Id;
            wooobj["manage_stock"] = true;
            wooobj["stock_quantity"] = variant.InventoryQuantity;


            List<Dictionary<string, object>> attributes = new List<Dictionary<string, object>>();
            foreach (var op in product.Options) {
                Dictionary<string, object> neki = new Dictionary<string, object>();
                neki["name"] = op.Name;
                neki["position"] = op.Position;
                neki["visible"] = true;
                neki["options"] = null;
                neki["option"] = new ShopifyAttributeHandler().GetOptionValue((int)op.Position, variant);
                attributes.Add(neki);
            }
            wooobj["attributes"] = attributes;

            return wooobj;
        }
    }
}
