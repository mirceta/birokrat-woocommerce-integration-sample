using BironextWordpressIntegrationHub.structs;
using BiroWooHub.logic.integration;
using System.IO;
using System.Threading.Tasks;
using tests.tools;

namespace tests.tests.estrada
{
    public class OrderAsserter
    {

        WooOrderToBiroDocumentComparator comparator;
        public OrderAsserter(WooOrderToBiroDocumentComparator comparator) {
            this.comparator = comparator;
        }

        public async Task<ComparisonResult> Assert(IIntegration integ, WoocommerceOrder order) {
            string tmp = Path.Combine(integ.Datafolder, order.Data.Number + ".xml");
            string some = File.ReadAllText(tmp);

            return await comparator.Compare(order, some, integ.ValidationComponents);
        }
    }
}
