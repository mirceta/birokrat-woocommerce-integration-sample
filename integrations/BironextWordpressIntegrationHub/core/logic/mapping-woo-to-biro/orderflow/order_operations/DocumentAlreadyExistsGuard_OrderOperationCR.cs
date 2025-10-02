using ApiClient.utils;
using BironextWordpressIntegrationHub;
using BironextWordpressIntegrationHub.structs;
using birowoo_exceptions;
using core.customers.zgeneric;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.orderflow.order_operations
{
    public class DocumentAlreadyExistsGuard_OrderOperationCR : IOrderOperationCR
    {

        IDocumentNumberGetter documentNumberGetter;
        BirokratDocumentType doctype;
        IOrderOperationCR next;

        public DocumentAlreadyExistsGuard_OrderOperationCR(IDocumentNumberGetter documentNumberGetter, BirokratDocumentType doctype, IOrderOperationCR next) {
            if (documentNumberGetter == null)
                throw new ArgumentNullException("documentNumberGetter");
            if (doctype == null)
                throw new ArgumentNullException("doctype");
            if (next == null) // next cannot be null in this case!
                throw new ArgumentNullException("next");
            this.documentNumberGetter = documentNumberGetter;
            this.doctype = doctype;
            this.next = next;
        }

        public async Task<Dictionary<string, object>> Next(WoocommerceOrder order, Dictionary<string, object> data) {

            var result = await documentNumberGetter.GetDocumentNumber(BironextApiPathHelper.GetStringByType(doctype), order);

            if (result.Success)
            {
                throw new DocumentAlreadyExistsException($"{doctype} already exists");
            }
            else if (result.ErrorMessage.Contains($"of additional number") &&
                     result.ErrorMessage.Contains("not found"))
            {
                return await next.Next(order, data);
            }
            else
            {
                // Handle other errors if necessary
                throw new Exception(result.ErrorMessage);
            }
        }
    }

    public class EnsureDocumentExists_OrderOperationCR : IOrderOperationCR
    {

        IDocumentNumberGetter documentNumberGetter;
        BirokratDocumentType doctype;
        IOrderOperationCR next;

        public EnsureDocumentExists_OrderOperationCR(IDocumentNumberGetter documentNumberGetter, BirokratDocumentType doctype, IOrderOperationCR next)
        {
            if (documentNumberGetter == null)
                throw new ArgumentNullException("documentNumberGetter");
            if (doctype == BirokratDocumentType.UNASSIGNED)
                throw new ArgumentNullException("doctype");
            if (next == null) // next cannot be null in this case!
                throw new ArgumentNullException("next");
            this.documentNumberGetter = documentNumberGetter;
            this.doctype = doctype;
            this.next = next;
        }

        public async Task<Dictionary<string, object>> Next(WoocommerceOrder order, Dictionary<string, object> data)
        {

            var result = await documentNumberGetter.GetDocumentNumber(BironextApiPathHelper.GetStringByType(doctype), order);

            if (result.Success)
            {
                throw new DocumentAlreadyExistsException($"{doctype} already exists");
            }
            else if (result.ErrorMessage.Contains($"of additional number") &&
                     result.ErrorMessage.Contains("not found"))
            {
                return await next.Next(order, data);
            }
            else
            {
                // Handle other errors if necessary
                throw new Exception(result.ErrorMessage);
            }
        }
    }
}
