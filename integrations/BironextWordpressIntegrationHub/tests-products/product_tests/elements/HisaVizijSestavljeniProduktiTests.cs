using BironextWordpressIntegrationHub.structs;
using BiroWooHub.logic.integration;
using core.tools.zalogaretriever;
using Newtonsoft.Json;
using si.birokrat.next.common.build;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using tests.tests.estrada;

namespace tests.tests.hisavizij {
    public class HisaVizijSestavljeniProduktiTests {

        string sestavljenArtikel;
        IZalogaRetriever zaloga;
        IIntegration integ;
        IMyLogger logger;
        public HisaVizijSestavljeniProduktiTests(string sestavljenArtikel, IZalogaRetriever zaloga, IIntegration integ, IMyLogger logger) {
            this.sestavljenArtikel = sestavljenArtikel;
            this.zaloga = zaloga;
            this.integ = integ;
            this.logger = logger;
        }

        public async Task SestavljeniProduktiTests() {
            // TUNED TO WORK ONLY FOR HISA VIZIJ!

            var start = (await zaloga.Query()).ToDictionary(x => x.Item1, x => x.Item2);

            var startZaloga = start[sestavljenArtikel];


            string path = Path.Combine(Build.SolutionPath, "tests_fixture",
                                    "jsons",
                                   "orders",
                                   "hisavizij",
                                   "narocila",
                                   "40018.json");
            string originalOrder = File.ReadAllText(path);
            var order = JsonConvert.DeserializeObject<WoocommerceOrder>(originalOrder);

            int sessionId = 87;
            var iadf = new ModifyOrderForTestingProcessor(sessionId,
                    new SetOrderStatusProcessor("processing",
                    new RetryingOrderProcessor(null, logger)));

            await iadf.Act(integ, order);


            var end = (await zaloga.Query()).ToDictionary(x => x.Item1, x => x.Item2);
            var endZaloga = end["sestavljenArtikel"];
            Console.WriteLine($"Start zaloga: {startZaloga} end zaloga: {endZaloga}");
        }
    }
}
