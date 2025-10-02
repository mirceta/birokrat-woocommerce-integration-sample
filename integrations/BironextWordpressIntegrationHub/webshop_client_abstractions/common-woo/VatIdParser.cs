using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using core.tools.wooops;
using gui_inferable;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.common_woo
{

    public interface IVatIdParser : IInferable
    {
        Task<string> Get(WoocommerceOrder order);
    }

    public class NopVatParser : IVatIdParser
    {
        public Dictionary<string, object> Infer(Dictionary<string, object> state)
        {
            state["vatIdParser"] = this;
            return state;
        }
        public async Task<string> Get(WoocommerceOrder order) {
            return "";
        }
    }

    public class VatNumberParser : IVatIdParser
    {
        public Dictionary<string, object> Infer(Dictionary<string, object> state)
        {
            state["vatIdParser"] = this;
            return state;
        }
        public async Task<string> Get(WoocommerceOrder order) {
            return VatIdHelper.GetVatIdFromMetadata(order, "_vat_number");
        }
    }

    public class YwEuVatIdParser : IVatIdParser
    {
        public Dictionary<string, object> Infer(Dictionary<string, object> state)
        {
            state["vatIdParser"] = this;
            return state;
        }
        public async Task<string> Get(WoocommerceOrder order) {
            return VatIdHelper.GetVatIdFromMetadata(order, "yweu_billing_vat");
        }
    }

    public class EstradaVatIdParser : IVatIdParser
    {

        IApiClientV2 client;

        public EstradaVatIdParser(IApiClientV2 client) {
            this.client = client;
        }

        public Dictionary<string, object> Infer(Dictionary<string, object> state)
        {
            state["vatIdParser"] = this;
            return state;
        }
        public async Task<string> Get(WoocommerceOrder order) {
            
            string davcna = await new YwEuVatIdParser().Get(order);

            davcna = davcna.Replace(" ", "");

            if (davcna.Contains("HR")) {
                return davcna;
            } else if (davcna.Contains("SI")) {
                return davcna;
            } else if (davcna.Contains("OIB")) {
                return davcna;
            }

            if (string.IsNullOrEmpty(davcna))
                return davcna;

            if (order.Data.Billing.Country == "HR") {
                if (GWooOps.IsVatExempt(order)) {
                    davcna = $"HR{davcna}";
                } else {
                    davcna = $"OIB{davcna}";
                }
            } else if (order.Data.Billing.Country == "SI"){

                string some = await client.utilities.DavcnaStevilka("utilities/partner/checkvatid", davcna);
                if (some.Length > 5) {
                    davcna = JsonConvert.DeserializeAnonymousType(some, new { DavcnaStevilka = "" }).DavcnaStevilka;
                }
                Console.WriteLine();

            
            }
            return davcna;
        }
    }

    public class VatIdHelper {
        public static string GetVatIdFromMetadata(WoocommerceOrder order, string attr_name) {
            var some = order.Data.MetaData.Where(x => x.Key == attr_name).ToList();
            string davcna = "";
            if (some.Count > 0) {
                var tmp = some.First().Value;
                davcna = Convert.ToString(tmp);
            }

            return davcna;
        }
    }
}
