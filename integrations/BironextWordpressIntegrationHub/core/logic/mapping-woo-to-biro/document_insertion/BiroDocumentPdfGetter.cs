using ApiClient.utils;
using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using core.customers.zgeneric;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.customers.poledancerka
{
    public class BiroDocumentPdfGetter : IAttachmentOperationCR
    {

        IAttachmentOperationCR next;
        IApiClientV2 client;
        public BiroDocumentPdfGetter(IApiClientV2 client, IAttachmentOperationCR next) {
            this.next = next;
            this.client = client;
        }

        public static async Task<string> GetPdfFromDocumentNumber(IApiClientV2 client, string documentNumber, string birokratDocumentType) {
            string racun_path = BironextApiPathHelper.GetVnosByType(birokratDocumentType);
            string pdf = await client.document.GetPdf(racun_path, documentNumber);
            var ano = new { content = "" };
            return JsonConvert.DeserializeAnonymousType(pdf, ano).content;
        }

        public async Task<string> Next(WoocommerceOrder order, Dictionary<string, object> data) {
            string doc = (string)data["tipDokumenta"];
            string num = (string)data["stevilkaDokumenta"];
            var tmp = await GetPdfFromDocumentNumber(client, num, doc);
            if (next != null) {
                data["pdf"] = tmp;
                return await next.Next(order, data);
            } else {
                return tmp;
            }
        }
    }
}
