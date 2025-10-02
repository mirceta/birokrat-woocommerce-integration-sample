using System.Collections.Generic;
using core.customers.zgeneric;
using System.Threading.Tasks;
using BironextWordpressIntegrationHub.structs;
using BirokratNext;
using core.tools.birokratops;
using pdf_handling;
using ApiClient.utils;

namespace gui_generator.api
{

    public class SavePdfOrderOperationCr : IOrderOperationCR
    {
        private IApiClientV2 client;
        private IOrderOperationCR next;
        private PdfDataDao dao;
        private int taskId;
        private string filepath = "";
        private string connectionString;
        public SavePdfOrderOperationCr(IApiClientV2 client, string connectionString, int taskId, IOrderOperationCR next)
        {
            this.client = client;
            this.dao = new PdfDataDao(connectionString);
            this.connectionString = connectionString;
            this.next = next;
            this.taskId = taskId;
        }



        public async Task<Dictionary<string, object>> Next(WoocommerceOrder order, Dictionary<string, object> data)
        {

            string stevilkaDokumenta = (string)data["stevilkaDokumenta"];
            string tipDokumenta = (string)data["tipDokumenta"];
            string vnos = BironextApiPathHelper.GetVnosByType(tipDokumenta);

            string content = await GBirokratOps.GetPdf(client, vnos, stevilkaDokumenta);

            await dao.InsertAsync(new PdfData()
            {
                BirokratDocNum = stevilkaDokumenta,
                BirokratDocType = tipDokumenta,
                Content = content,
                TaskId = taskId
            });

            if (next != null)
            {
                return await next.Next(order, data);
            }
            return data;
        }
    }



}