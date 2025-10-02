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
    public class WooSimpleProductRetriever : IOutProductRetriever
    {


        ProductRetrieverAsyncOperationFactory asyncOperationFactory;
        int asyncBatchSize;
        public WooSimpleProductRetriever(ProductRetrieverAsyncOperationFactory asyncOperationFactory, int asyncBatchSize = 10)
        {
            this.asyncOperationFactory = asyncOperationFactory;
            this.asyncBatchSize = asyncBatchSize;
        }

        public List<Dictionary<string, object>> Get(IOutApiClient integ)
        {

            string some = integ.GetKita($"products?per_page=100\\\"&\\\"page={1}").GetAwaiter().GetResult();
            if (some.Contains("page is not of type integer"))
            {
                var x = integ.GetKita($"products").GetAwaiter().GetResult();
                var tmp = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<List<Dictionary<string, object>>>(x);
                NormalizeWeirdWooTypes.normalizeIds(tmp);
                return tmp;
            }
            else
            {
                return GetInParallel(integ);
            }
        }

        

        private List<Dictionary<string, object>> GetInParallel(IOutApiClient integ)
        {
            ConcurrentBag<Dictionary<string, object>> chome = new ConcurrentBag<Dictionary<string, object>>();

            ProductRetrieverWorkloadCreator bc = new ProductRetrieverWorkloadCreator(asyncOperationFactory, integ);
            List<Task> workload = bc.CreateBatch(chome);

            var cond = new NoNewItemsTerminationCondition<Dictionary<string, object>>(chome);

            BatchAsyncExecutor executor = new BatchAsyncExecutor(asyncBatchSize, cond);
            executor.Execute(workload);

            return chome.ToList();
        }
    }

    public class ProductRetrieverWorkloadCreator : IBatchCreator<ConcurrentBag<Dictionary<string, object>>>
    {

        ProductRetrieverAsyncOperationFactory asyncOperationFactory;
        IOutApiClient wooclient;

        public ProductRetrieverWorkloadCreator(ProductRetrieverAsyncOperationFactory asyncOperationFactory,
            IOutApiClient wooclient)
        {
            this.asyncOperationFactory = asyncOperationFactory;
            this.wooclient = wooclient;
        }

        public List<Task> CreateBatch(ConcurrentBag<Dictionary<string, object>> accumulator)
        {
            List<Task> batch = new List<Task>();
            for (int i = 1; i <= 1000; i++)
            {
                IAsyncOperation ctx = asyncOperationFactory.Create(wooclient, i, accumulator);
                Task t = new Task(() =>
                {
                    ctx.Work();
                });
                batch.Add(t);
            }
            return batch;
        }
    }

    public class ProductRetrieverAsyncOperation : IAsyncOperation
    {
        public int page = 0;
        ConcurrentBag<Dictionary<string, object>> accumulator;
        IOutApiClient woo;

        public ProductRetrieverAsyncOperation(IOutApiClient woo, int page, ConcurrentBag<Dictionary<string, object>> accumulator)
        {
            this.page = page;
            this.accumulator = accumulator;
            this.woo = woo;
        }

        public void Work()
        {
            string some = woo.GetKita($"products?per_page=100\\\"&\\\"page=\"{page}").GetAwaiter().GetResult();
            var wooidsofsearchresult = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<List<Dictionary<string, object>>>(some);
            NormalizeWeirdWooTypes.normalizeIds(wooidsofsearchresult);
            wooidsofsearchresult.ForEach(x => accumulator.Add(x));
        }
    }

    public class ProductRetrieverAsyncOperationFactory
    {
        ILogger logger;
        public ProductRetrieverAsyncOperationFactory(ILogger logger)
        {
            this.logger = logger;
        }

        public IAsyncOperation Create(IOutApiClient woo, int page, ConcurrentBag<Dictionary<string, object>> accumulator)
        {
            ProductRetrieverAsyncOperation ctx = new ProductRetrieverAsyncOperation(woo, page, accumulator);
            return new RetryingAsyncOperation(ctx, new DotnetStandardLoggerWrapper(logger));
        }
    }

    public class NormalizeWeirdWooTypes {
        public static void normalizeIds(List<Dictionary<string, object>> tmp)
        {
            foreach (var t in tmp)
            {
                t["id"] = GWooOps.SerializeIntWooProperty(t["id"]);
                t["product_id"] = GWooOps.SerializeIntWooProperty(t["id"]);
            }
        }
    }
}
