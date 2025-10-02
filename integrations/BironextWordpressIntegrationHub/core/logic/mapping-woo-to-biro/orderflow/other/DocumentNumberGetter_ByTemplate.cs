using ApiClient.utils;
using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using birowoo_exceptions;
using core.customers.zgeneric;
using gui_attributes;
using JsonIntegrationLoader.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BironextWordpressIntegrationHub
{
    public class DocumentNumberGetter_ByTemplate : IDocumentNumberGetter, IAttachmentOperationCR {

        IApiClientV2 client;
        OrderAttributeTemplateParser2 template;

        IAttachmentOperationCR next;
        string birokratDocumentType;


        [GuiConstructor]
        public DocumentNumberGetter_ByTemplate(IApiClientV2 client, OrderAttributeTemplateParser2 template, string birokratDocumentType, IAttachmentOperationCR next)
        {
            this.client = client;
            this.template = template;
            this.next = next;
            this.birokratDocumentType = birokratDocumentType;
        }

        public DocumentNumberGetter_ByTemplate(IApiClientV2 client, OrderAttributeTemplateParser2 template)
        {
            this.client = client;
            this.template = template;
        }

        private async Task<DocumentNumberResult> get_document_number(string birokratDocumentType, WoocommerceOrder order) {
            
            var pak = new Dictionary<string, object>();

            var pars = await client.cumulative.Parametri(BironextApiPathHelper.GetCumulativeByDocumentType(birokratDocumentType), null);
            Dictionary<string, object> postback = pars.ToDictionary(x => x.Koda, y => y.PrivzetaVrednost);
            postback["OdIzstavitve"] = $"{DateTime.Now.Year - 1}-01-01";

            string sourceDocumentAdditionalNumber = template.Parse(order);
            postback[BironextApiPathHelper.GetCumulativeAdditionalNumberParameter(birokratDocumentType)] = "*" + sourceDocumentAdditionalNumber + "*";
           
            var res = await client.cumulative.Podatki(BironextApiPathHelper.GetCumulativeByDocumentType(birokratDocumentType), postback);

            string stevilkaDataField = BironextApiPathHelper.GetCumulativeData_DocNumberField(birokratDocumentType);
            var result = res.Where(x => {
                try {
                    string some = BironextApiPathHelper.GetCumulativeData_DocAdditionalNumberField(birokratDocumentType);
                    return ((string)x[some]).Trim() == sourceDocumentAdditionalNumber;
                } catch (BirokratDocumentTypeNotSupported ex) {
                    string some = BironextApiPathHelper.GetCumulativeData_DocNumberField(birokratDocumentType);
                    string chome = ((string)x[some]).Trim();
                    return chome.Contains(sourceDocumentAdditionalNumber);
                }
            })
                .Select(x => {
                    string num = ((string)x[stevilkaDataField]);
                    // PROBLEM TO NE BO DELALO PROV KER JE DOSTKRAT VEC MINUSOV V STEVILKI!
                    if (num.Contains("-")) { // recimo pri predracunu je v polju tudi dodatna stevilka kar je odvec
                        num = num.Substring(0, num.IndexOf("-"));
                    }
                    return num;
                }).ToList();
            if (result.Count == 1)
            {
                return DocumentNumberResult.SuccessResult(result.Single());
            }
            else if (result.Count == 0)
            {
                return DocumentNumberResult.FailureResult($"{birokratDocumentType} of additional number {sourceDocumentAdditionalNumber} not found");
            }
            else
                throw new IntegrationProcessingException($"Multiple examples of {birokratDocumentType} have the additional number {sourceDocumentAdditionalNumber}");
        }

        #region [IDocumentNumberGetter]
        public async Task<DocumentNumberResult> GetDocumentNumber(string birokratDocumentType, WoocommerceOrder order) {
            return await get_document_number(birokratDocumentType, order);
        }
        #endregion

        #region [IAttachmentOperationCR]
        

        public async Task<string> Next(WoocommerceOrder order, Dictionary<string, object> data) {

            string doctype = birokratDocumentType;
            data["tipDokumenta"] = doctype;
            data["stevilkaDokumenta"] = await get_document_number(doctype, order);
            if (next != null) {
                return await next.Next(order, data);
            } else {
                return (string)data["stevilkaDokumenta"];
            }

        }
        #endregion
    }
}
