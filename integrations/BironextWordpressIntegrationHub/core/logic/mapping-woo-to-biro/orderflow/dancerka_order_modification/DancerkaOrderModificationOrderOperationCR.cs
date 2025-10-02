using ApiClient.utils;
using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub;
using BiroWoocommerceHub.flows;
using core.customers.zgeneric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.order_operations
{
    public class DancerkaOrderModificationOrderOperationCR : IOrderOperationCR {


        IApiClientV2 client;
        IOrderOperationCR next;
        ICountryMapper countryMapper;
        public DancerkaOrderModificationOrderOperationCR(IApiClientV2 client, ICountryMapper countryMapper, IOrderOperationCR next) {
            this.client = client;
            this.next = next;
            this.countryMapper = countryMapper;
        }

        public async Task<Dictionary<string, object>> Next(WoocommerceOrder order, Dictionary<string, object> data) {

            string documentType = (string)data["tipDokumenta"];
            string documentNumber = (string)data["stevilkaDokumenta"];

            string apiPath = BironextApiPathHelper.GetVnosByType(documentType);

            string woobillingcountry = order.Data.Billing.Country;
            string wooshippingcountry = order.Data.Shipping.Country;

            string biroshippingcountry = wooshippingcountry;
            if (countryMapper != null)
                biroshippingcountry = await countryMapper.Map(wooshippingcountry);

            // update
            var pake = (await client.document.UpdateParameters(apiPath, documentNumber))
               .GroupBy(x => x.Koda)
               .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);

            var pak = new Dictionary<string, object>();
            pak["txtUvodniText"] = pake["txtUvodniText"];
            pak["Klavzula"] = pake["Klavzula"];

            pak["cmbPredloga"] = "ARRacun1"; // privzeta predloga

            

            // jezik racuna
            if (new string[] { "SI", "SLO" }.ToList().Contains(wooshippingcountry)) {
                pak["txtUvodniText"] = $"Na osnovi naročila: #{order.Data.Number}";
                pak["cmbJezik"] = "002 Slovenščina";
            } else {
                pak["txtUvodniText"] = $"Based on order: #{order.Data.Number}";
                pak["cmbJezik"] = "003 Angleščina";
            }


            // nacin obracunavanja ddv
            if (!new string[] { "SI", "SLO" }.ToList().Contains(wooshippingcountry)) {
                pak["DrzavaDDV"] = biroshippingcountry;
            }

            // klavzula
            string finaltext1 = "";
            if (new string[] { "SI", "SLO" }.ToList().Contains(wooshippingcountry)) {
                finaltext1 = (string)pak["Klavzula"];
                finaltext1 = finaltext1.Replace("Thank you for your custom!", "Hvala za nakup!");
                pak["Klavzula"] = finaltext1;
            } else if (Tools.IsEUWooCountry(wooshippingcountry)) { }
            else {
                finaltext1 = (string)pak["Klavzula"];
                finaltext1 = finaltext1.Replace("\r\rThank you for your custom!\r\r", "");
                finaltext1 += "Oproščeno DDV po točki a) prvega odstavka 52. člena ZDDV-1\r";
                finaltext1 += "VAT exempt under Article 146(1)(a) of Directive\r";
                finaltext1 += "\r";
                finaltext1 += "Thank you for your custom!";

                pak["Klavzula"] = finaltext1;
            }


            await client.document.Update(apiPath, documentNumber, pak);

            pak = new Dictionary<string, object>();
            if (!new string[] { "SI", "SLO" }.ToList().Contains(wooshippingcountry)) {
                pak["cmbVrstaProdaje"] = "e-Trgovanje";
                await client.document.Update(apiPath, documentNumber, pak);
            }
            if (next != null) {
                return await next.Next(order, data);
            }
            return data;
        }
    }
}
