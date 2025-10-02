using biro_to_woo_common.executor.context_processor;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace tests.tests.hisavizij
{
    class ProductTestsWooDeleter {

        IIntegration integ;
        public ProductTestsWooDeleter(IIntegration integration) {
            this.integ = integration;
        }

        public async Task<List<Dictionary<string, object>>> FullDeleteAllProductsFromWebshop(CancellationToken token)
        {
            var deletor = new WebshopDeleteVarProds_ThenReturnSifras();

            await deletor.DeleteVariableProductsFromWoocommerce(integ);

            var chome = await new BirokratGetVarProds(integ).GetVariableProductsFromBirokrat();
            await deletor.DeleteProductsRetryingBySku(integ, chome, token);


            var prods = await integ.WooClient.GetProducts();
            List<string> skus = prods.Select(x => (string)x["sku"]).ToList();
            await deletor.DeleteProductsRetryingBySku(integ, skus, token);

            prods = await integ.WooClient.GetProducts();

            return prods;
        }

        public async Task FullDeleteProductsWithSkusFromWebshop(IComparisonContextCreator testset, CancellationToken token)
        {
            var tmp = await testset.Create(integ, token);
            string skufield = BirokratNameOfFieldInFunctionality.SifrantArtiklov(integ.BiroToWoo.SkuBirokratField);

            var deletor = new WebshopDeleteVarProds_ThenReturnSifras();

            try
            {
                await deletor.DeleteProductsRetryingBySku(integ, tmp.biroItems.Select(x => (string)x[skufield]).ToList(), token);
            }
            catch (NullReferenceException ex) {
                Console.WriteLine("");
            }
        }
    }

    
}
