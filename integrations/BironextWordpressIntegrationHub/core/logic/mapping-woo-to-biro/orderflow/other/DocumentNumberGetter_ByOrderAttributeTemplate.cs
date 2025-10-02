using ApiClient.utils;
using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using core.customers.zgeneric;
using JsonIntegrationLoader.utils;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using tests.tools;

namespace BironextWordpressIntegrationHub
{

    public class DocumentNumberGetter_ByOrderAttributeTemplate : IDocumentNumberGetter, IAttachmentOperationCR
    {
        private readonly DocumentNumberGetter_ByTemplate _decorated;


        private IApiClientV2 client;
        private string documentAdditionalNumberTemplate;
        private BirokratDocumentType birokratDocumentType;
        private IAttachmentOperationCR next;

        public DocumentNumberGetter_ByOrderAttributeTemplate(IApiClientV2 client, string documentAdditionalNumberTemplate, 
            BirokratDocumentType birokratDocumentType, IAttachmentOperationCR next)
        {

            this.client = client;
            this.documentAdditionalNumberTemplate = documentAdditionalNumberTemplate;
            this.birokratDocumentType = birokratDocumentType;
            this.next = next;

            _decorated = new DocumentNumberGetter_ByTemplate(client, 
                new OrderAttributeTemplateParser2(documentAdditionalNumberTemplate), 
                BironextApiPathHelper.GetStringByType(birokratDocumentType), next);
        }

        public async Task<DocumentNumberResult> GetDocumentNumber(string birokratDocumentType, WoocommerceOrder order)
        {   
            return await _decorated.GetDocumentNumber(birokratDocumentType, order);
        }

        public async Task<string> Next(WoocommerceOrder order, Dictionary<string, object> data)
        {
            return await _decorated.Next(order, data);
        }
    }

    public class DocumentNumberGetter_ByOrderAttributeTemplate2 : IDocumentNumberGetter, IAttachmentOperationCR
    {
        private readonly DocumentNumberGetter_ByTemplate _decorated;


        IApiClientV2 client;
        string documentAdditionalNumberTemplate;
        public DocumentNumberGetter_ByOrderAttributeTemplate2(IApiClientV2 client, string documentAdditionalNumberTemplate)
        {
            this.client = client;
            this.documentAdditionalNumberTemplate = documentAdditionalNumberTemplate;

            _decorated = new DocumentNumberGetter_ByTemplate(client,
                new OrderAttributeTemplateParser2(documentAdditionalNumberTemplate));
        }

        public async Task<DocumentNumberResult> GetDocumentNumber(string birokratDocumentType, WoocommerceOrder order)
        {
            return await _decorated.GetDocumentNumber(birokratDocumentType, order);
        }

        public async Task<string> Next(WoocommerceOrder order, Dictionary<string, object> data)
        {
            return await _decorated.Next(order, data);
        }
    }
}
