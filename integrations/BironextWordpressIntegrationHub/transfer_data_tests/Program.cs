using administration_data.data.structs;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using transfer_data.sql_accessors.order_transfer_creator;
using validator.logic.order_transfer.accessor;
using validator;
using webshop_client_woocommerce;
using validator.logic;
using Microsoft.CodeAnalysis.CSharp;
using gui_generator.api;
using tests.composition.common;
using tests.composition.fixed_task.common;
using gui_generator_integs.final_adapter;
using transfer_data.orders.sql_accessors;
using si.birokrat.next.common.logging;

namespace transfer_data_tests
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            string url = "https://belisa.si";
            string ck = "ck_85e33fcefea11b99632adb002b67753ef62a8f61";
            string cs = "cs_e0d52efb754b155cf3f3fa4a3187d89692a4d0dc";
            string apikey = "SO3onPC7AhmrSgI54J6uNDKEfYmFpJlk+Ze7nskPQQw=";

            IOutApiClient wooclient = new WooApiClient(new WoocommerceCaller_NetworkFailureGuard(5, new WoocommerceRESTPythonCaller(url, ck, cs, "wc/v3", 2)));


            string connectionString = @"Server=localhost\MSSQLSERVER01;Database=integrations_db;Trusted_Connection=True;";

            string bironextAddress = "http://localhost:19000/api/";


            var adapterFactory = new LazyIntegrationAdapterBuilder();
            adapterFactory.withBironext(bironextAddress);
            adapterFactory.withEnforcedParameters(new OutClientEnforcingParameters()
            {
                enforcedClient = null,
                enforceBiroToWoo = false,
                enforceWooToBiro = false
            });
            adapterFactory.withIntegDataFolder(@"C:\Users\Administrator\Desktop\integrations_data");
            var adapter = adapterFactory.Create();



            
            var get = new SqlIntegrationFactory(
                        new ProductionVersionPicker(
                            new administration_data.IntegrationDao(connectionString),
                            new administration_data.IntegrationVersionDao(connectionString)),
                        new SqlAdministrationData_LazyIntegrationBuilder(connectionString,
                        adapterFactory.Create()));



            var lazy = get.GetLazy("BELISA.SI");
            var lazyIntegration = await lazy;

            int integId = int.Parse(lazyIntegration.AdditionalInfo["integrationId"]);
            int verId = int.Parse(lazyIntegration.AdditionalInfo["versionId"]);

            var integration = await lazyIntegration.BuildIntegrationAsync();

            var validator = new ValidatorSynchronization(
                                            new List<IIntegration> { integration },
                                            new OrderTransferProcessorRoot_SimpleGuard(),
                                            new transfer_data.system.OrderTransferSystemFactory(connectionString),
                                            new ConsoleMyLogger());

            /*
             * new OrderTransferAccessorFactory((integration) =>
                                            {
                                                // OVER HERE NEEDS TO BE SQL OTA!
                                                IOrderTransferAccessor baseAccessor = new SqlOrderTransferAccessor(
                                                    connectionString,
                                                    integrationId: integId, 
                                                    integration.WooClient);
                                                return baseAccessor;
                                            })
             */

            /*
             INTEGRATION ID AND VERSION ID NEEDS TO BE INJECTED IN BOTH CREATOR AND OTACCESSOR!
             
             */



            OrderTransferDao otdao = new OrderTransferDao(connectionString);

            // don't forget you need to inject integrationId AND integrationVersion into here!
            var creator = new WoocommerceOrderTransferCreator(
                new TimeSpan(15, 0, 0, 0),
                new DateTime(2023, 8, 20),
                wooclient,
                otdao,
                integId: integId,
                versionId: verId,
                eventHooks: new List<string> { "processing", "completed" });

            await creator.CreateNewOrderTransfers();
            await validator.Work();
        }
    }
}
