
using BiroWooHub.logic.integration;
using core.customers;
using Newtonsoft.Json;
using si.birokrat.next.common.build;
using si.birokrat.next.common.logging;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using webshop_client_woocommerce.product_retriever;

namespace products_to_excel
{


    class Program {
        static async Task Main(string[] args)
        {
            string bironextAddress = "https://next.birokrat.si/api/";   //"http://localhost:19000/api/";//"https://next.birokrat.si/api/";
            string integration_data_path = Path.Combine(Build.ProjectPath, @"appdata");
            string python_path = null;
            var coll = new PredefinedIntegrationFactory(false, bironextAddress, integration_data_path, python_path);
            var integration = await coll.GetLazyByName("POLEDANCERKA_WOOTOBIRO_PRODUCTION");
            await new KompletiGenerator(await integration.BuildIntegrationAsync(), true, new ConsoleMyLogger()).Execute();
        }
    }


    public interface ISourceDataRetriever {
        List<Dictionary<string, object>> GetSourceData(IIntegration integration);
    }
    public class SourceDataRetriever : ISourceDataRetriever {
        public List<Dictionary<string, object>> GetSourceData(IIntegration integration)
        {
            WooProductRetriever retr = new WooProductRetriever(10, null);
            var products = retr.Get(integration.WooClient);

            return products;
        }
    }

    public class Cached : ISourceDataRetriever
    {
        public const string cachefile = "source_data_cache.json";
        SourceDataRetriever next;
        public Cached(SourceDataRetriever data) {
            this.next = data;
        }
        public List<Dictionary<string, object>> GetSourceData(IIntegration integration)
        {
            if (!File.Exists(cachefile))
            {
                var products = next.GetSourceData(integration);

                string json = JsonConvert.SerializeObject(products);
                File.WriteAllText(cachefile, json);
                return products;
            }
            else
            {
                var tmp = File.ReadAllText(cachefile);
                return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(tmp);
            }
        }

        public static void KillCache() { 
            if (File.Exists(cachefile)) {
                File.Delete(cachefile);
            }
        }
    }

    public class BiroSetArtikel {
        public string name;
        public string sifra;
        public List<string> original_item_sifras;
    }
}
