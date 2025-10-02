using biro_to_woo.logic.change_trackers.exhaustive;
using BiroWoocommerceHubTests;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests_fixture
{
    internal class OrderRetrieverWorkloadCreator : IBatchCreator<ConcurrentBag<OrderDescription>>
    {

        OrderRetrieverModeFactory asyncOperationFactory;
        IOutApiClient wooclient;

        public OrderRetrieverWorkloadCreator(OrderRetrieverModeFactory asyncOperationFactory,
            IOutApiClient wooclient) {
            this.asyncOperationFactory = asyncOperationFactory;
            this.wooclient = wooclient;
        }

        public List<Task> CreateBatch(ConcurrentBag<OrderDescription> accumulator) {
            List<Task> batch = new List<Task>();
            for (int i = 1; i < 100; i++) {
                IAsyncOperation ctx = asyncOperationFactory.Create(wooclient, i, accumulator);
                Task t = new Task(() => {
                    ctx.Work();
                });
                batch.Add(t);
            }
            return batch;
        }
    }
}
