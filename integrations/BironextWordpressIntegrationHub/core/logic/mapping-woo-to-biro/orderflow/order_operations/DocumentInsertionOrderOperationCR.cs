using ApiClient.utils;
using BironextWordpressIntegrationHub.structs;
using core.logic.mapping_woo_to_biro.document_insertion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.customers.zgeneric
{
    public class DocumentInsertionOrderOperationCR : IOrderOperationCR {

        DocumentInsertion doc;
        IOrderOperationCR next;
        BirokratDocumentType doctype;
        public DocumentInsertionOrderOperationCR(DocumentInsertion doc, BirokratDocumentType doctype, IOrderOperationCR next) {
            this.doc = doc;
            this.next = next;
            this.doctype = doctype;
        }

        public async Task<Dictionary<string, object>> Next(WoocommerceOrder order, Dictionary<string, object> data) {

            string partnerBirokratId = (string)data["partnerBirokratId"]; 
            string stevilkaDokumenta = await doc.InsertDocument(order, partnerBirokratId);

            int t = -1;
            if (!int.TryParse(stevilkaDokumenta, out t)) {
                var anon = new { documentNumber = "" };
                try {
                    stevilkaDokumenta = JsonConvert.DeserializeAnonymousType(stevilkaDokumenta, anon).documentNumber;
                } catch (Exception ex) {
                    throw new System.Exception(stevilkaDokumenta, ex);
                }
            }

            data["tipDokumenta"] = BironextApiPathHelper.GetStringByType(doctype);
            data["stevilkaDokumenta"] = stevilkaDokumenta;
            if (next != null) {
               return await next.Next(order, data);
            }
            return data;
        }
    }
}