using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.customers;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using tests.tools.order_transfer_tests;
using tests_webshop.products;
using transfer_data.system;
using transfer_data_abstractions.orders;
using validator;
using validator.logic;

namespace tests_webshop {
    class Program {
        static async Task Main(string[] args)
        {
            string bironextaddress = "https://next.birokrat.si/api/";
            string datafolder = "";
            PredefinedIntegrationFactory integrations = new PredefinedIntegrationFactory(true, bironextaddress, datafolder);


            var integ = await (await integrations.GetLazyByName("MENHART_WOOTOBIRO_STAGING")).BuildIntegrationAsync();
            var x = integ.WooClient;
            var accessor = await new OrderTransferSystemFactory("").Get(integ).GetOrderTransferAccessor(integ);



            var ot = await accessor.Get("16690", "processing");
            ot.OrderTransferStatus = OrderTransferStatus.UNVERIFIED;
            await accessor.Set(ot);

            //resetOrderTransfers(accessor);

        }

        private async Task resetOrderTransfers(IOrderTransferAccessor accessor)
        {
            var list = await accessor.GetByStatus(null);
            list = list.Where(x => int.Parse(x.OrderId) >= 280700 && int.Parse(x.OrderId) <= 280736).ToList();

            list.ForEach(async x =>
            {
                await accessor.Delete(x.OrderId, x.OrderStatus);
                await accessor.AddUnaccepted(x.OrderId, x.OrderStatus);
            });
        }

        async Task tests(IOrderTransferAccessor accessor, IOutApiClient x, IIntegration integ) {

            var tests = new WebshopAccessTests(accessor);

            tests.Cleanup();
            tests.OrderTransfer_Tests();
            tests.Cleanup();

            integ.ObvezneNastavitve.Verify(integ.BiroClient).GetAwaiter().GetResult();
        }
    }
}
