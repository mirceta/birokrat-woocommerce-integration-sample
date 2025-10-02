using ApiClient.utils;
using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub;
using core.customers.zgeneric;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.order_operations.pl
{
    public class DocumentParametersModifierOrderOperationCR : IOrderOperationCR {

        List<DocumentParameterCommand> commands;
        IApiClientV2 client;
        string documentApiPath;
        IOrderOperationCR next;
        ICountryMapper countryMapper;

        public DocumentParametersModifierOrderOperationCR(IApiClientV2 client, List<DocumentParameterCommand> commands, 
            ICountryMapper countryMapper, IOrderOperationCR next = null) {
            this.commands = commands;
            this.client = client;
            this.next = next;
            this.countryMapper = countryMapper;

        }

        public async Task<Dictionary<string, object>> Next(WoocommerceOrder order, Dictionary<string, object> data) {

            string woobillingcountry = order.Data.Billing.Country;
            string wooshippingcountry = order.Data.Shipping.Country;

            string biroshippingcountry = wooshippingcountry;
            if (countryMapper != null)
                biroshippingcountry = await countryMapper.Map(wooshippingcountry);
            data["woobillingcountry"] = woobillingcountry;
            data["wooshippingcountry"] = wooshippingcountry;
            data["biroshippingcountry"] = biroshippingcountry;


            string documentType = (string)data["tipDokumenta"];
            string documentNumber = (string)data["stevilkaDokumenta"];
            string apiPath = BironextApiPathHelper.GetVnosByType(documentType);

            var tmp = new DocumentParameterManager(client, apiPath, documentNumber);
            await tmp.ExecuteChain(order, data, commands);

            if (next != null) {
                return await next.Next(order, data);
            }
            return data;
        }
    }
}
