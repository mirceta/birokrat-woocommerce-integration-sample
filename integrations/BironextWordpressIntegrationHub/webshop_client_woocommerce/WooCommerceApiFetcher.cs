using biro_to_woo.logic.change_trackers.exhaustive;
using BirokratNext;
using BiroWoocommerceHubTests;
using common_abstractions_core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tests_fixture;

namespace transfer_data.sql_accessors.order_transfer_creator.deps
{

    public class WooCommerceApiFetcher
    {
        private readonly IOutApiClient _apiClient;
        private readonly int _batchSize;

        public WooCommerceApiFetcher(IOutApiClient apiClient, int batchSize = 20)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _batchSize = batchSize;
        }

        public async Task<List<string>> FetchAllOrderNotes(IEnumerable<int> orderIds)
        {

            int batchSize = 20;
            ConcurrentBag<string> accumulator = new ConcurrentBag<string>();
            var bc = new NoteWorkloadCreator(_apiClient, orderIds.ToList());
            List<Task> workload = bc.CreateBatch(accumulator);
            var cond = new NopTerminationCondition();
            BatchAsyncExecutor executor = new BatchAsyncExecutor(20, cond);
            executor.Execute(workload);
            var result = accumulator.ToList();



            return result;
        }
    }

    internal class NoteWorkloadCreator : IBatchCreator<ConcurrentBag<string>>
    {

        IOutApiClient wooclient;
        List<int> orderIds;

        public NoteWorkloadCreator(
            IOutApiClient wooclient,
            List<int> orderIds)
        {
            this.wooclient = wooclient;
            this.orderIds = orderIds;
        }

        public List<Task> CreateBatch(ConcurrentBag<string> accumulator)
        {
            List<Task> batch = new List<Task>();
            foreach (var id in orderIds)
            {
                IAsyncOperation ctx = new NoteRetrieveOp(wooclient, orderId: id, accumulator);
                Task t = new Task(() => {
                    ctx.Work();
                });
                batch.Add(t);
            }
            return batch;
        }

    }

    internal class NoteRetrieveOp : IAsyncOperation
    {
        public int page = 0;
        ConcurrentBag<string> accumulator;
        IOutApiClient woo;
        int orderId;

        public NoteRetrieveOp(IOutApiClient woo, int orderId, ConcurrentBag<string> accumulator)
        {
            this.orderId = orderId;
            this.accumulator = accumulator;
            this.woo = woo;
        }

        public void Work()
        {
            var url = $"orders/{orderId}/notes";

            try
            {
                var content = woo.GetKita(url).GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(content))
                {
                    accumulator.Add(content);
                }
                else
                {
                    accumulator.Add("");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Api client call failed: {ex.Message}", ex);
            }
        }
    }

}