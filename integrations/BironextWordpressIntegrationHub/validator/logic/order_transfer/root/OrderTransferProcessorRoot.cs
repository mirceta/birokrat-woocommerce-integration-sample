using BiroWooHub.logic.integration;
using transfer_data_abstractions.orders;
using validator.logic;
using validator.logic.order_transfer.guard;
using validator.logic.order_transfer_processor;

namespace validator
{
    public class OrderTransferProcessorRoot_SimpleGuard : IOrderTransferProcessorRootFactory{
        public OrderTransferProcessorRoot_SimpleGuard() { 
        
        }

        public IOrderTransferProcessorRoot Create(IIntegration integration,
            IOrderTransferAccessor accessor,
            OrderTransferStatus orderTransferStatus,
            IOrderPostprocessor postprocessor) {
            
            var mainProcessor = new OrderTransferProcessorStageFactory().Create(integration,
                accessor,
                orderTransferStatus,
                postprocessor);

            return new SimpleOrderTransferProcessorGuard(mainProcessor,
                accessor,
                new ThreadSleepOperation(180 * 1000));

        }
    }
}
