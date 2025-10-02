using BironextWordpressIntegrationHub.structs;
using BiroWooHub.logic.integration;
using core.customers;
using Newtonsoft.Json;
using si.birokrat.next.common.build;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using tests.tests.estrada;
using tests.tools;
using transfer_data.sql_accessors.order_transfer_creator.deps;
using transfer_data.system;

namespace woo_to_biro_single
{
    class Program {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            string bironextAddress = "http://localhost:19000/api/";   //"http://localhost:19000/api/";//"https://next.birokrat.si/api/";
            string integration_data_path = Path.Combine(Build.ProjectPath, @"appdata");
            string python_path = null;


            await singleDriverOldStyle(bironextAddress);
            
            
            //await singleDriverFromSql(bironextAddress);
        }

        private static async Task singleDriverOldStyle(string bironextAddress) {
            string integration_data_path = Path.Combine(Build.ProjectPath, @"appdata");
            string python_path = null;
            var coll = new PredefinedIntegrationFactory(debug: true, bironextAddress, integration_data_path, python_path);
            var lazyIntegration = await coll.GetLazyByName("POLEDANCERKA_WOOTOBIRO_PRODUCTION");

            var integration = await lazyIntegration.BuildIntegrationAsync();
            var x = integration.WooClient;

            var accessor = await new OrderTransferSystemFactory("").Get(integration).GetOrderTransferAccessor(integration);

            List<string> some = new List<string>() { "59978" };

            List<string> statuses = new List<string>() { "processing" };
            foreach (string s in some)
            {
                foreach (string status in statuses)
                {
                    string odr = await accessor.GetOrder(s);

                    var o = JsonConvert.DeserializeObject<WoocommerceOrder>(odr);
                    o.Data.Status = status;
                    o.Data.Number = o.Data.Number + "177";
                    o.Data.Id = o.Data.Id + 177;
                    odr = JsonConvert.SerializeObject(o);
                    try
                    {
                        await integration.WooToBiro.OnOrderStatusChanged(odr);
                        //await ValidatorIteration(integration, o);
                    }
                    catch (Exception ex) { }
                }
            }
        }


        private static async Task InsertOrderSingle(IIntegration integration)
        {

            /*
            string path = @"C:\Users\vucko\Desktop\playground\bironext-woocommerce-integration\BironextWordpressIntegrationHub\tests_fixture\jsons\orders\spica\simple.json";
            string odr = File.ReadAllText(path);
        
            var order = JsonConvert.DeserializeObject<WoocommerceOrder>(odr);
            odr = JsonConvert.SerializeObject(order);
            */
            //await ValidatorIteration(integration, order);
            List<string> some = new List<string>() { "57749", };
            List<string> statuses = new List<string>() { "on-hold" };
            // lycon-shop List<string> some = new List<string>() { "15207", "15206", "15205", "15202" };
            // lycon List<string> some = new List<string>() { "14104", "14103", "14102", "14101", "14100", "14099", "14098" };



            foreach (string s in some)
            {
                foreach (string status in statuses)
                {
                    string odr = await integration.WooClient.MyGetOrder(s);

                    var o = JsonConvert.DeserializeObject<WoocommerceOrder>(odr);
                    o.Data.Status = status;
                    o.Data.Number = o.Data.Number;
                    o.Data.Id = o.Data.Id;
                    odr = JsonConvert.SerializeObject(o);
                    await integration.WooToBiro.OnOrderStatusChanged(odr);
                    await new OrderAsserter(new WooOrderToBiroDocumentComparator(false, new SkipCompare()
                    {
                        Country = false
                    })).Assert(integration, o);
                }


            }
        }
    }
}
