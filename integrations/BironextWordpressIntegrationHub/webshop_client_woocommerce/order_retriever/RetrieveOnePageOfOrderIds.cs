using biro_to_woo.logic.change_trackers.exhaustive;
using BiroWoocommerceHubTests;
using System.Collections.Concurrent;

namespace tests_fixture
{
    internal class RetrieveOnePageOfOrderIds : IAsyncOperation
    {
        public int page = 0;
        ConcurrentBag<OrderDescription> accumulator;
        IOutApiClient woo;

        public RetrieveOnePageOfOrderIds(IOutApiClient woo, int page, ConcurrentBag<OrderDescription> accumulator)
        {
            this.page = page;
            this.accumulator = accumulator;
            this.woo = woo;
        }

        public void Work()
        {
            var anon = new[] { new { id = "", date_created = "", status = "" } };
            var tmp = woo.GetKita($"orders?dp=6\'&\'per_page=100\'&\'page={page}").GetAwaiter().GetResult(); // !!!!!! SOLUTION TO THE DECIMALS PROBLEM!!!!
            var orders = OrderDescriptionDeserializer.Deserialize(tmp);
            orders.ForEach(x => accumulator.Add(x));
        }
    }
}
