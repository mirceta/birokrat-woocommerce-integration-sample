using BiroWooHub.logic.integration;
using core.customers;
using core.tools.zalogaretriever;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace tests.tests.hisavizij
{

    // These tests only test integration.onArticleChange(), they do not test BiroToWooExecutor
    public class FullProductTests : ITests<string> {
        IIntegration integration;
        LazyIntegration lazyIntegration;
        IProductTestsetRetriever testset;

        IZalogaRetriever zaloga;
        string sestavljenArtikel = null;

        IMyLogger logger;

        public FullProductTests(
            IMyLogger logger,
            IProductTestsetRetriever testset,
            LazyIntegration integ,
            string sestavljenArtikel = null) {
            this.logger = logger;
            this.lazyIntegration = integ;
            this.sestavljenArtikel = sestavljenArtikel;
            this.testset = testset;
        }
        public async Task Work(CancellationToken token) {

            integration = await lazyIntegration.BuildIntegrationAsync.Invoke();
            zaloga = integration.ValidationComponents.Zaloga;

            await ProductTransfering();
            if (sestavljenArtikel != null) {
                await new HisaVizijSestavljeniProduktiTests(sestavljenArtikel, zaloga, integration, logger).SestavljeniProduktiTests();
            }
        }

        public async Task ProductTransfering() {

            var verifier = new VerifyProduct(logger, integration, zaloga, integration.ValidationComponents.TestEqualAdditions, null);
            var tests = new ProductTests(integration, logger, zaloga, verifier);

            var sifre = await testset.Get(integration);

            //new WooProductDeleter(integration.WooClient).DeleteAllProducts();

            // first pass to test on add
            await tests.Should_ChangePrice_ChangeZaloga_AndSynchronize(sifre);

            // second pass to test on change
            await tests.Should_ChangePrice_ChangeZaloga_AndSynchronize(sifre);

            results = tests.ReturnResults();
        }


        List<string> results = new List<string>();
        public string GetResult() {
            return string.Join(Environment.NewLine, results);
        }
    }
}
