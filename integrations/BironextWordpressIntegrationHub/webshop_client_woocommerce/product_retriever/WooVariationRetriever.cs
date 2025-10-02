using biro_to_woo.logic.change_trackers.exhaustive;
using BiroWoocommerceHubTests;
using common_abstractions_core;
using core.tools.wooops;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webshop_client_woocommerce.product_retriever
{
    public class WooVariationRetriever
    {


        VariationRetrieverAsyncOperationFactory asyncOperationFactory;
        int asyncBatchSize;
        public WooVariationRetriever(VariationRetrieverAsyncOperationFactory asyncOperationFactory, int asyncBatchSize = 10)
        {
            this.asyncOperationFactory = asyncOperationFactory;
            this.asyncBatchSize = asyncBatchSize;
        }

        public List<Dictionary<string, object>> Get(IOutApiClient integ, List<string> variableProductIds)
        {

            ConcurrentBag<Dictionary<string, object>> chome = new ConcurrentBag<Dictionary<string, object>>();

            List<Task> workload = CreateBatch(integ, variableProductIds, chome);

            var cond = new NopTerminationCondition();

            BatchAsyncExecutor executor = new BatchAsyncExecutor(asyncBatchSize, cond);
            executor.Execute(workload);

            var lst = chome.ToList().OrderBy(x => GWooOps.SerializeIntWooProperty(x["id"])).ToList();

            return lst;
        }

        public List<Task> CreateBatch(IOutApiClient wooclient,
            List<string> variableProductIds,
            ConcurrentBag<Dictionary<string, object>> accumulator)
        {
            List<Task> batch = new List<Task>();
            foreach (var x in variableProductIds)
            {
                IAsyncOperation ctx = asyncOperationFactory.Create(wooclient, x, accumulator);
                Task t = new Task(() =>
                {
                    ctx.Work();
                });
                batch.Add(t);
            }
            return batch;
        }
    }

    public class VariationRetrieverAsyncOperation : IAsyncOperation
    {
        public string variableProductId;
        ConcurrentBag<Dictionary<string, object>> accumulator;
        IOutApiClient woo;

        public VariationRetrieverAsyncOperation(IOutApiClient woo, string variableProductId, ConcurrentBag<Dictionary<string, object>> accumulator)
        {
            this.variableProductId = variableProductId;
            this.accumulator = accumulator;
            this.woo = woo;
        }

        public void Work()
        {
            string some = woo.GetKita($"products/{variableProductId}/variations?per_page=100").GetAwaiter().GetResult();
            var search_results = new JsonPowerDeserialization()
                .DeserializeObjectImmuneToBadJSONEscapeSequenece<List<Dictionary<string, object>>>(some);
            search_results = search_results
                                .Select(x => { x["parent_id"] = variableProductId; return x; })
                                .ToList();
            search_results.ForEach(x => accumulator.Add(x));
        }
    }

    public class VariationRetrieverAsyncOperationFactory
    {
        ILogger logger;
        public VariationRetrieverAsyncOperationFactory(ILogger logger)
        {
            this.logger = logger;
        }

        public IAsyncOperation Create(IOutApiClient woo, string variableProductId, ConcurrentBag<Dictionary<string, object>> accumulator)
        {
            VariationRetrieverAsyncOperation ctx = new VariationRetrieverAsyncOperation(woo, variableProductId, accumulator);
            return new RetryingAsyncOperation(ctx, new DotnetStandardLoggerWrapper(logger));
        }
    }
}
