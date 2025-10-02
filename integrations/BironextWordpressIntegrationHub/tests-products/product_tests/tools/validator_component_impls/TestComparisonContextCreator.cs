using biro_to_woo_common.executor.context_processor;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.structs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace tests.tests.hisavizij
{
    public class TestComparisonContextCreator : IComparisonContextCreator {

        List<string> sifras;
        public TestComparisonContextCreator(List<string> sifras) {
            this.sifras = sifras;
        }

        public async Task<BiroOutComparisonContext> Create(IIntegration integration, CancellationToken token) {

            var biroArtikelRetriever = integration.BiroToWoo.GetBirokratArtikelRetriever();

            List<Dictionary<string, object>> biroItems;
            var addAttrs = integration.BiroToWoo.GetVariationAttributes();
            var queryTerms = addAttrs.ToList().ToDictionary(x => x.Key, x => (object)true);
            var artikli = await biroArtikelRetriever.Query(queryTerms, null);

            artikli = artikli.Where(x => sifras.Contains(((string)x[BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla)]))).ToList();

            var context = new BiroOutComparisonContext();
            context.biroItems = artikli;

            context.outItems = await integration.WooClient.GetProducts();

            return context;
        }
    }

    public class Test2ComparisonContextCreator : IComparisonContextCreator
    {
        IProductTestsetRetriever retriever;
        public Test2ComparisonContextCreator(IProductTestsetRetriever retriever) {
            this.retriever = retriever;
        }

        public async Task<BiroOutComparisonContext> Create(IIntegration integration, CancellationToken token)
        {
            var sifre = await retriever.Get(integration);
            return await new TestComparisonContextCreator(sifre).Create(integration, token);
        }
    }
}
