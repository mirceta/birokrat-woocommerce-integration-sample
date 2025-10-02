using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tests_webshop.products;
using validator;

namespace transfer_data
{
    public class Class1
    {
        /*
         Used to be implemented!
         public Task<string> MyGetOrder(string id);
        public Task<string> GetOrderTransfers();
        public Task<string> GetOrderTransfer(string orderTransfer);
        public Task<string> AddUnacceptedOrderTransfer(string orderId, string orderStatus);
        public Task<string> DeleteOrderTransfer(string orderid, string orderstatus);
        public Task<string> PutOrderTransfer(OrderTransfer orderTransfer);


        public Task<string> PostOrPutProductTransfer(ProductTransfer productTransfer);
        public Task<List<ProductTransfer>> ListProductTransfers();
        public Task DeleteProductTransfer(string productid);




         */

        /* Shopify implementation
         
        public Task<string> PutOrderTransfer(OrderTransfer orderTransfer) {
            throw new NotImplementedException();
        }

        public async Task<string> PostOrPutProductTransfer(ProductTransfer productTransfer) {
            string path = getProductTransferSignature();

            List<ProductTransfer> pts = new List<ProductTransfer>();
            if (File.Exists(path)) {
                string content = File.ReadAllText(path);
                pts = JsonConvert.DeserializeObject<List<ProductTransfer>>(content);
            }

            var tmp = pts.Where(x => x.product_id == productTransfer.product_id).ToList();
            if (tmp.Count == 1) {
                ProductTransfer current = tmp.Single();
                pts.Remove(current);
            }

            pts.Add(productTransfer);

            File.WriteAllText(path, JsonConvert.SerializeObject(pts));
            return "200";
        }

        public async Task<List<ProductTransfer>> ListProductTransfers() {
            string path = getProductTransferSignature();
            if (File.Exists(path)) {
                string content = File.ReadAllText(path);
                var pts = JsonConvert.DeserializeObject<List<ProductTransfer>>(content);
                return pts; 
            }
            return new List<ProductTransfer>();
        }

        private string getProductTransferSignature() {
            string tmp1 = storeUrl.Replace(":", "").Replace(".", "").Replace("/", "");
            string tmp2 = access_token.Replace(":", "").Replace(".", "").Replace("/", "");
            string res = tmp1 + tmp2 + "prodtransfers.json";
            return Path.Combine(Build.SolutionPath, res);
        }



         */
    }
}
