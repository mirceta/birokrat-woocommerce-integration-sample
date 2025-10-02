using System.Collections.Generic;
using System.Threading.Tasks;
using validator;
using BiroWooHub.logic.integration;
using validator.logic.order_transfer.accessor;
using common_birowoo;
using si.birokrat.next.common.logging;
using tests.composition.common;
using si.birokrat.next.common.database;
using System.Net;
using System;
using transfer_data.sql_accessors.order_transfer_creator;
using transfer_data.sql_accessors;
using BiroWoocommerceHubTests;
using transfer_data.orders.sql_accessors;
using transfer_data_abstractions.orders;
using transfer_data.system;

namespace tests_tasks.production.specific
{
    public class OrderProduction
    {

        OrderTransferSystemFactory otsfactory;
        public OrderProduction(OrderTransferSystemFactory otsfactory)
        {
            this.otsfactory = otsfactory;
        }

        public async Task Execute(SimpleDecoratingFactory<IIntegration, IOrderTransferAccessor> orderDecorator, IMyLogger logger, IIntegration integration)
        {
            await Execute_SingleIter_ConstantWebshopOTA(orderDecorator, logger, integration);
        }

        async Task Execute_SingleIter_ConstantWebshopOTA(
            SimpleDecoratingFactory<IIntegration, IOrderTransferAccessor> orderDecorator, 
            IMyLogger logger, 
            IIntegration integration) {

            var validator = new ValidatorSynchronization(
                                            new List<IIntegration> { integration },
                                            new OrderTransferProcessorRoot_SimpleGuard(),
                                            otsfactory,
                                            logger);
            /* WARNING - OrderTransferSystemFactory is no longer decoratable but it should be!!!
             * new OrderTransferAccessorFactory((integ) =>
                {
                    IOrderTransferAccessor baseAccessor = new WoocommerceWebshopOrderTransferAccessor(integration.WooClient);
                    if (orderDecorator != null)
                        baseAccessor = orderDecorator.Decorate(integ, baseAccessor);
                    return baseAccessor;
                })
             */


            var start = new RandomDelayedStart(integration.Name, integration.BiroClient, logger);
            await start.Prod(async () =>
                    await validator.Work());
        }

        /*
        async Task Execute_SingleIter_Constant_OTAIsLocalSql(
            SimpleDecoratingFactory<IIntegration, IOrderTransferAccessor> orderDecorator,
            IMyLogger logger,
            IIntegration integration) {


            int integId = 0;
            string connectionString = "currently_not_working";
            IOutApiClient wooclient = null;
            int verId = 0;

            var validator = new ValidatorSynchronization(
                                            new List<IIntegration> { integration },
                                            new OrderTransferProcessorRoot_SimpleGuard(),
                                            new OrderTransferAccessorFactory((integ) =>
                                            {
                                                // OVER HERE NEEDS TO BE SQL OTA!
                                                IOrderTransferAccessor baseAccessor = new SqlOrderTransferAccessor(
                                                    connectionString,
                                                    integrationId: integId,
                                                    integration.WooClient);
                                                return baseAccessor;
                                            }));

            OrderTransferDao otdao = new OrderTransferDao(connectionString);

            // HERE WE NEED TO INSERT ADDITIONAL PARAMS!

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
        */
    }
}
