using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHubTests;
using core.customers;
using core.tools.wooops;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace order_mapping
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            string bironextaddress = "http://next.birokrat.si/api/";
            string datafolder = "";
            PredefinedIntegrationFactory integrations = new PredefinedIntegrationFactory(true, bironextaddress, datafolder);



            string filename = "some.txt";
            string some = null;

            string filenameprocessed = "chome.txt";
            string chome = null;


            //File.Delete(filename);
            //File.Delete(filenameprocessed);
            if (File.Exists(filename) && File.Exists(filenameprocessed))
            {
                some = File.ReadAllText(filename);
                chome = File.ReadAllText(filenameprocessed);
            }
            else
            {
                var integ = await (await integrations.GetLazyByName("POLEDANCERKA_WOOTOBIRO_PRODUCTION")).BuildIntegrationAsync();
                var wooclient = integ.WooClient;
                some = await wooclient.Get($"orders/{53931}");


                var obj1 = JObject.Parse(some);
                await new WoocommerceOrderFormatTransformer(null).TransformStageOne_InternetDependent(wooclient, obj1);

                some = obj1.ToString();
                File.WriteAllText(filename, some);

                chome = await wooclient.MyGetOrder("53931");
                File.WriteAllText($"{filenameprocessed}", chome);
            }

            var obj = JObject.Parse(some);
            obj = await new WoocommerceOrderFormatTransformer(null).TransformStageTwo_InternetIndependent(obj);
            string outputJson = obj.ToString();

            var procobj = JObject.Parse(chome);

            procobj["coupons"] = null;

            var result = new JObjectDifferenceFinder().FindDifference(procobj, obj).ToString();

            Console.WriteLine(result.ToString());

            var asdfasdf = new JsonPowerDeserialization2().Deserialize<WoocommerceOrder>(outputJson);

            Console.WriteLine(result.ToString());
        }
    }
}
