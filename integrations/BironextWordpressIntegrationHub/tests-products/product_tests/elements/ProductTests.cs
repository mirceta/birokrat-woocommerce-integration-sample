using biro_to_woo.logic.change_trackers.exhaustive;
using birowoo_exceptions;
using BiroWoocommerceHub.structs_wc_to_biro;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.tools.zalogaretriever;
using Newtonsoft.Json;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace tests {

    public class ProductTests {

        IIntegration integ;
        IMyLogger logger;
        VerifyProduct verifier;
        IZalogaRetriever zaloga;
        public ProductTests(IIntegration integ,
            IMyLogger logger,
            IZalogaRetriever zaloga,
            VerifyProduct verifier) {
            this.integ = integ;
            this.logger = logger;
            this.verifier = verifier;
            this.zaloga = zaloga;
        }

        public async Task Should_ChangePrice_ChangeZaloga_AndSynchronize(List<string> sifre) {

        }


        List<string> results = new List<string>();
        public List<string> ReturnResults() {
            return results;
        }

    }

    
}
