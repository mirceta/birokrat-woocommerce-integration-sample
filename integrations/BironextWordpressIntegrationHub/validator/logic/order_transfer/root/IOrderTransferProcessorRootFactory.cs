using BiroWooHub.logic.integration;
using transfer_data_abstractions.orders;
using validator.logic;
using validator.logic.order_transfer_processor;

namespace validator
{
    public interface IOrderTransferProcessorRootFactory {
        public IOrderTransferProcessorRoot Create(IIntegration integration,
            IOrderTransferAccessor accessor,
            OrderTransferStatus orderTransferStatus,
            IOrderPostprocessor postprocessor);
    }
}
