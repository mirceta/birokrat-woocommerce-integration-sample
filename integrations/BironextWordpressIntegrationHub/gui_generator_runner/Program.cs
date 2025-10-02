using BiroWooHub.logic.integration;
using core.customers;
using gui_gen;
using gui_generator.api;
using gui_generator.comparison;
using gui_generator_integs.final_adapter;
using Newtonsoft.Json;
using si.birokrat.next.common.build;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gui_generator
{
    public class Program {
        static async Task Main(string[] args) {

            var original1 = GetFromFactory("SPICA_BIROTOWOO_PRODUCTION");

            // to JSON
            var o1_integration = await original1;
            var curval1 = new LazyIntegrationAdapter().Adapt(o1_integration);

            var tmp11 = IntegrationConfigTools.NullImplementationOptions(curval1);
            var tmp111 = JsonConvert.SerializeObject(tmp11);


            List<string> originals = new List<string>()
            {
                "POLEDANCERKA_WOOTOBIRO_PRODUCTION",
                "PARTYPEK_WOOTOBIRO_PRODUCTION",
                "PARTYPEK_BIROTOWOO_PRODUCTION",
                "EIGRACE_BIROTOWOO_PRODUCTION",
                "HISAVIZIJ_BIROTOWOO_PRODUCTION",
                "HISAVIZIJ_WOOTOBIRO_PRODUCTION",
                "SPICA_WOOTOBIRO_PRODUCTION",
                "SPICA_BIROTOWOO_PRODUCTION",
                "POLEDANCERKAB2B_WOOTOBIRO_PRODUCTION"
            };


            foreach (var integrationName in originals)
            {
                // get from PRE
                var original = GetFromFactory(integrationName);

                // to JSON
                var o2_integration = await original;
                var curval = new LazyIntegrationAdapter().Adapt(o2_integration);

                var tmp1 = IntegrationConfigTools.NullImplementationOptions(curval);
                var tmp = JsonConvert.SerializeObject(tmp1);


                // back to PRE
                string bironextaddress = "https://next.birokrat.si/api/";
                var adapterFactory = new LazyIntegrationAdapterBuilder();
                adapterFactory.withBironext(bironextaddress);
                adapterFactory.withEnforcedParameters(new OutClientEnforcingParameters()
                {
                    enforcedClient = null,
                    enforceBiroToWoo = false,
                    enforceWooToBiro = false
                });
                adapterFactory.withIntegDataFolder(@"C:\Users\Administrator\Desktop\integrations_data");
                var adapter = adapterFactory.Create();

                var adapted = adapter.AdaptFinal(curval, "BIROTOWOO");


                // compare of successful:
                ObjectComparer.TreatNullAndEmptyAsEqual = true;
                ObjectComparer.AreEquivalent(original, adapted);

                Console.WriteLine($"{integrationName}: equivalent");
            }

        }

        static PredefinedIntegrationFactory predefIntegFactory = null;
        static async Task<IIntegration> GetFromFactory(string integrationName) {
            string bironextAddress = "https://next.birokrat.si/api/";   //"http://localhost:19000/api/";//"https://next.birokrat.si/api/";
            string integration_data_path = Path.Combine(Build.ProjectPath, @"appdata");
            string python_path = null;

            if (predefIntegFactory == null)
                predefIntegFactory = new PredefinedIntegrationFactory(debug: true, bironextAddress, integration_data_path, python_path);
            
            var lazyIntegration = await predefIntegFactory.GetLazyByName(integrationName);
            var integration = await lazyIntegration.BuildIntegrationAsync.Invoke();
            return integration;
        }
    }

}