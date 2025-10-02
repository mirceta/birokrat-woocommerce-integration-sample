using BiroWoocommerceHub.structs_wc_to_biro;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.tools.birokratops;
using core.tools.wooops;
using core.tools.zalogaretriever;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using tests.tests.hisavizij;
using tests.tools;

namespace tests {
    public class VerifyProduct {
        IMyLogger logger;
        IIntegration integ;
        IZalogaRetriever zaloga;
        List<TestEqualAddition> additionalTests;
        TracedList results;
        public VerifyProduct(IMyLogger logger, IIntegration integ, IZalogaRetriever zaloga, List<TestEqualAddition> additionalTests, TracedList results) {
            this.logger = logger;
            this.integ = integ;
            this.zaloga = zaloga;
            this.additionalTests = additionalTests;
            this.results = results;
        }

        public async Task Verify(string sifra)
        {
            try
            {
                await Inner(sifra);
            }
            catch (Exception ex) {
                Debugger.Break();
            }
        }

        private async Task Inner(string sifra)
        {
            string skufield = BirokratNameOfFieldInFunctionality.SifrantArtiklov(integ.BiroToWoo.SkuBirokratField);


            string varfield = "";
            if (integ.BiroToWoo.VariableProductBirokratField != BirokratField.None)
                varfield = BirokratNameOfFieldInFunctionality.SifrantArtiklov(integ.BiroToWoo.VariableProductBirokratField);


            var birokratObj = await GBirokratOps.GetAndBuildBirokratArtikel(integ.BiroClient, zaloga, sifra);

            var wooItemRoot = new Dictionary<string, object>();
            var wooItem = await GWooOps.GetWooProductBySku(integ.WooClient, (string)birokratObj[skufield]);



            WrapResults(TestUtils.TestEqual(birokratObj, skufield, wooItem, "sku"));
            WrapResults(TestUtils.TestEqual(birokratObj, "PCsPD", wooItem, "regular_price", "dbl"));
            WrapResults(TestUtils.TestEqual(birokratObj, "zaloga", wooItem, "stock_quantity", "int"));

            if (!string.IsNullOrEmpty((string)birokratObj[varfield]))
            {
                wooItemRoot = await GWooOps.GetWooProductBySku(integ.WooClient, (string)birokratObj[varfield]);
                WrapResults(TestUtils.TestEqual(birokratObj, varfield, wooItemRoot, "sku"));
            }

            if (additionalTests != null)
            {
                foreach (var test in additionalTests)
                {
                    string type = "str";
                    if (test.outFieldType == OutFieldType.VARIABLE_ATTRIBUTE)
                    {
                        if (string.IsNullOrEmpty((string)birokratObj[varfield]))
                            continue; // skip if not variable product! 
                        type = "varattr";
                    }

                    WrapResults(TestUtils.TestEqual(birokratObj, test.biroField, wooItem, test.outField, type));
                }
            }

            // kategorij ne preverjamo pri variaciji
            //TestUtils.TestEqual(birokratObj, "ComboVrsta", wooItemVariation, "Vrsta");
            //TestUtils.TestEqual(birokratObj, "ComboPodVrsta", wooItemVariation, "Podvrsta");
        }

        private void WrapResults(string result) {
            var color = "green";
            if (!result.StartsWith("PASS"))
                color = "red";

            results.Add($"<{color}>");
            results.Add(result);
            results.Add($"</{color}>");
            logger.LogInformation(result);
        }
    }

    
}
