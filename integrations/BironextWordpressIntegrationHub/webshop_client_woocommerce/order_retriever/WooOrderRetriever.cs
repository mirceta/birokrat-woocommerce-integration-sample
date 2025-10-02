using biro_to_woo.logic.change_trackers.exhaustive;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHubTests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using webshop_client_woocommerce;

namespace tests_fixture
{

    internal class WooOrderRetriever
    {
        OrderRetrieverModeFactory asyncOperationFactory;
        int asyncBatchSize;
        int maxdaysold = 1800;
        int maxCount = 500000;
        public WooOrderRetriever(OrderRetrieverModeFactory asyncOperationFactory, int asyncBatchSize = 5) {
            this.asyncOperationFactory = asyncOperationFactory;
            this.asyncBatchSize = asyncBatchSize;
        }

        public void SetMaxDaysOld(int maxdaysold) {
            this.maxdaysold = maxdaysold;
        }

        public void SetMaxCount(int maxCount) { 
            this.maxCount = maxCount;
        }

        public List<OrderDescription> Get(IOutApiClient integ)
        {
            var some = integ.GetKita($"orders?per_page=100\'&\'orderby=date\'&\'order=desc").GetAwaiter().GetResult();
            var descs = OrderDescriptionDeserializer.Deserialize(some);


            List<OrderDescription> result = new List<OrderDescription>();
            // Now termination condition uposchtevs multiple termination conditions when retrieving lots of data
            // parallel, while the
            // first sequential retrieve still only looks at the time boundary.
            if (descs.All(x => orderIsNewEnoughToGet(x))) // if all are new enough, then next page will likely be new enough too...
            {
                ConcurrentBag<OrderDescription> accumulator = new ConcurrentBag<OrderDescription>();
                OrderRetrieverWorkloadCreator bc = new OrderRetrieverWorkloadCreator(asyncOperationFactory, integ);
                List<Task> workload = bc.CreateBatch(accumulator);
                var cond = GetTerminationCondition(accumulator, maxdaysold, maxCount);
                BatchAsyncExecutor executor = new BatchAsyncExecutor(asyncBatchSize, cond);
                executor.Execute(workload);
                result = accumulator.ToList();
            }
            else {
                // first page is enough, we don't need parallelizm
                result = descs.Where(x => orderIsNewEnoughToGet(x)).ToList();
            }

            var anon = new { id = "", date_created = "" };

            if (result.Count() > 0)
            {
                result = result.Where(x => DateTime.Now.Subtract(new TimeSpan(maxdaysold, 0, 0, 0)) <
                                                DateTime.ParseExact(x.date_created, "yyyy-MM-dd", CultureInfo.InvariantCulture))
                                .ToList();

                result = result.OrderByDescending(x => x.date_created).Take(Math.Min(maxCount, result.Count())).ToList();
            }

            return result;
        }

        ITerminationCondition GetTerminationCondition(ConcurrentBag<OrderDescription> accumulator, 
                                                      int maxDaysOld,
                                                      int maxCount) {

            var timeThresh = new OlderThanXDays(accumulator, maxDaysOld);
            var countThresh = new MoreThanMaxCountOfItems(accumulator, maxCount);
            var noNewItems = new NoNewItemsTerminationCondition<OrderDescription>(accumulator);

            return new OrTerminationCondition(new List<ITerminationCondition> { 
                    timeThresh,
                    countThresh, 
                    noNewItems
                });
        }

        private bool orderIsNewEnoughToGet(OrderDescription x)
        {
            return DateTime.Now.Subtract(DateTime.ParseExact(x.date_created, "yyyy-MM-dd", CultureInfo.InvariantCulture)).Days < maxdaysold;
        }
    }
}
