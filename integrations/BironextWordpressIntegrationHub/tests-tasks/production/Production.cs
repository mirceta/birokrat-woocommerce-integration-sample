using System;
using System.Threading.Tasks;

using tests_webshop.products;
using BiroWooHub.logic.integration;
using common_birowoo;
using si.birokrat.next.common.logging;
using core.customers;
using System.Net;
using tests.interfaces;
using tests.tests.estrada;
using System.Threading;
using tests.composition.final_composers.tests;
using si.birokrat.next.common.database;
using transfer_data.sql_accessors.order_transfer_creator;
using transfer_data.sql_accessors;
using tests_tasks.production.specific;
using transfer_data_abstractions.orders;
using transfer_data.system;

namespace tests.composition.final_composers.production
{

    public class ProductionFactory : IActualWorkFactory
    {

        OrderTransferSystemFactory otsfactory;
        SimpleDecoratingFactory<IIntegration, IOrderTransferAccessor> orderDecorator;
        public ProductionFactory(OrderTransferSystemFactory otsfactory, SimpleDecoratingFactory<IIntegration, IOrderTransferAccessor> orderDecorator) { 
            this.orderDecorator = orderDecorator;
            this.otsfactory = otsfactory;
        }
        public IActualWork Create()
        {
            return new Production(otsfactory, orderDecorator);
        }
    }

    public partial class Production : IActualWork
    {

        SimpleDecoratingFactory<IIntegration, IOrderTransferAccessor> orderDecorator;


        // actually not used here?? We only added this because some other code asked for it.
        object addParams;
        public object AdditionalParams { get => addParams; set => addParams = value; }

        public string Result => "";


        OrderTransferSystemFactory otsfactory;
        public Production(OrderTransferSystemFactory otsfactory, SimpleDecoratingFactory<IIntegration, IOrderTransferAccessor> orderDecorator)
        {
            this.otsfactory = otsfactory;
            this.orderDecorator = orderDecorator;
        }

        public async Task Tests(SimpleDecoratingFactory<IIntegration, IProductTransferAccessor> productDecorator, 
            SimpleDecoratingFactory<IIntegration, IOutcomeHandler> orderDecoratorFactory, 
            LazyIntegration lazyIntegration, 
            TestEnvironmentParams testenv, 
            IMyLogger logger, 
            CancellationToken cancellationToken)
        {
            var integration = await lazyIntegration.BuildIntegrationAsync.Invoke();
            if (lazyIntegration.Type == "BIROTOWOO")
            {
                await new ProductProduction().Execute(productDecorator, logger, integration);
            }
            else if (lazyIntegration.Type == "WOOTOBIRO")
            {
                await new OrderProduction(otsfactory).Execute(orderDecorator, logger, integration);
            }
            else
            {
                throw new Exception("Integration type not recognized");
            }
        }
    }
}
