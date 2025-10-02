using ApiClient.utils;
using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using core.tools.wooops;
using JsonIntegrationLoader.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.customers.zgeneric.order_operations
{

    public class ChangeDocNumOrderOperationCR : IOrderOperationCR
    {
        // legacy class for compatibility with old params
        ChangeDocNumOrderOperationCR2 tmp;


        IApiClientV2 bironext; 
        string template;
        IOrderOperationCR next;

        public ChangeDocNumOrderOperationCR(IApiClientV2 bironext, string template, IOrderOperationCR next)
        {
            this.bironext = bironext;
            this.template = template;
            this.next = next;

            tmp = new ChangeDocNumOrderOperationCR2(bironext, new OrderAttributeTemplateParser2(template), next);
        }

        public async Task<Dictionary<string, object>> Next(WoocommerceOrder order, Dictionary<string, object> data)
        {
            return await tmp.Next(order, data);
        }
    }

    public class ChangeDocNumOrderOperationCR2 : IOrderOperationCR {

        IApiClientV2 bironext;
        IOrderOperationCR next;
        OrderAttributeTemplateParser2 template;

        public ChangeDocNumOrderOperationCR2(IApiClientV2 bironext, OrderAttributeTemplateParser2 template, IOrderOperationCR next) {
            this.bironext = bironext;
            this.next = next;
            this.template = template;
        }

        public async Task<Dictionary<string, object>> Next(WoocommerceOrder order, Dictionary<string, object> data) {

            string doctype = (string)data["tipDokumenta"];
            string docnr = (string)data["stevilkaDokumenta"];
            string documentApiPath = BironextApiPathHelper.GetVnosByType(doctype);
            var some = await bironext.document.UpdateParameters(documentApiPath, docnr, null);
            var pars = some
           .GroupBy(x => x.Koda)
           .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);

            var pak = new Dictionary<string, object>();

            string res = template.Parse(order);

            pak["DodatnaStevilka"] = res;
            await bironext.document.Update(documentApiPath, docnr, pak);

            if (next != null) {
                return await next.Next(order, data);
            }
            return data;
        }
    }
}
