using allintegrations_factories.customers.estrada;
using biro_to_woo_common.executor;
using BirokratNext;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.structs;
using si.birokrat.next.common.logging;
using transfer_data.system;
using webshop_client_woocommerce;
using wpplugingen;

namespace demonstration_project
{
    internal class Program
    {

        static string plugin_deployment_path = @"C:\Users\birowoo-oldest-dev\desktop\woodeployment";

        public static async Task Main(string[] args)
        {
            string pythonpath = null; // set the python path (e.g. python 3.8 path to python.exe). install woocommerce library via pip install woocommerce!
            string biroApiKey = "";

            /*
             First generate a plugin using the project wpplugingen!
            */
            await CreatePhpPlugin(biroApiKey);

            

            /*
             Create the integration
             */
            var client = new ApiClientV2("https://next.birokrat.si/api", biroApiKey, 120);

            string address = "https://konecreative.eu/";
            string ck = "ck_24487f6811af50e42fd7dd709743ba3b0d428272";
            string cs = "cs_ba5ac93395728c97eb74dbc7e775f23e708abb47";
            string version = "wc/v3";
            var wooclient1 = new WooApiClient(new WoocommerceCaller_NetworkFailureGuard(5,
                        new WoocommerceRESTPythonCaller(address, ck, cs, version, 2, pythonpath)));

            // You can check out the other possible factories in allintegrations-factories.csproj
            // or build your own by seeing the template within there.
            var factory = new EstradaIntegrationFactory(true, "C:/some");
            var integration = await factory.BuildIntegration(client, wooclient1, biroApiKey);


            /*
             Use the integration to transfer an order from woocommerce to birokrat
             */
            string orderId = "";

            var accessor = await (new PureWoocommerceOrderTransferSystem()).GetOrderTransferAccessor(integration);
            var order = await accessor.GetOrder(orderId);

            await integration.WooToBiro.OnOrderStatusChanged(order);


            /*
             Use the integration to synchronize stock and prices from birokrat to woocommerce
             depending on the configuration of the integration
             */
            var birotowoo = new BiroToWooExecutorFactory(new ConsoleMyLogger()).SingleIterationTesting(integration);
            await birotowoo.Execute(integration, new CancellationToken());
        }

        private static async Task CreatePhpPlugin(string biroApiKey)
        {
            /*
             You need to upload the plugin onto your wordpress site to register additional REST routes
             required by the integration.
             */
            var phpconfig = new PhpPluginConfig() // you dont need any of these
            {
                ProductHooks = false,
                AcceptableAttachmentOrderStatuses = null,
                AttachmentHook = false,
                OrderStatusHooks = null
            };

            await new WppluginGen(plugin_deployment_path).createPlugin(phpconfig, biroApiKey);
        }
    }
}
