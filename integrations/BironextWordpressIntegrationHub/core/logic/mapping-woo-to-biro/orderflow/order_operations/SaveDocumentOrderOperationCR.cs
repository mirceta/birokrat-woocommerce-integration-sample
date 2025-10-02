using ApiClient.utils;
using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.flows;
using core.customers.zgeneric;
using core.tools.birokratops;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.order_operations
{
    public class SaveDocumentOrderOperationCR : IOrderOperationCR {

        IApiClientV2 client;
        IOrderOperationCR next;
        string filepath = "";
        public SaveDocumentOrderOperationCR(IApiClientV2 client, IOrderOperationCR next, string filepath = "") {
            this.client = client;
            this.next = next;
            this.filepath = filepath;
        }
        public async Task<Dictionary<string, object>> Next(WoocommerceOrder order, Dictionary<string, object> data) {

            string stevilkaDokumenta = (string)data["stevilkaDokumenta"];
            string tipDokumenta = (string)data["tipDokumenta"];
            string vnos = BironextApiPathHelper.GetVnosByType(tipDokumenta);

            await GBirokratOps.GetAndSavePdf(client, vnos, stevilkaDokumenta, order.Data.Number, filepath);

            for (int i = 0; i < 5; i++) {
                try {
                    string pdf = await client.document.GetEslog(vnos, stevilkaDokumenta);
                    if (string.IsNullOrEmpty(pdf))
                        continue;
                    File.WriteAllText(Path.Combine(filepath, $"{order.Data.Number}.xml"), pdf);
                    break;
                } catch (Exception ex) {
                    if (i == 2) throw ex;
                }
            }

            if (next != null) {
                return await next.Next(order, data);
            }
            return data;
        }
    }
}
