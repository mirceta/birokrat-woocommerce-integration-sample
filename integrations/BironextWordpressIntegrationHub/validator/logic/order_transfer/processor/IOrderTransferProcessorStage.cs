using BiroWooHub.logic.integration;
using System;
using System.Text;
using System.Threading.Tasks;
using transfer_data_abstractions.orders;

namespace validator.logic
{


    public class OrderTransferProcessorStageFactory {
        public OrderTransferProcessorStageFactory() { }

        public IOrderTransferProcessorStage Create(IIntegration integration,
            IOrderTransferAccessor accessor,
            OrderTransferStatus status,
            IOrderPostprocessor postprocessor) {
            switch (status) {
                case OrderTransferStatus.UNACCEPTED:
                case OrderTransferStatus.ACCEPTED:
                    return new UnacceptedAcceptedOrderTransferProcessorStage(integration.WooToBiro, accessor, postprocessor);
                case OrderTransferStatus.UNVERIFIED:
                    return new UnverifiedOrderTransferProcessorStage(integration.ValidationComponents, integration.BiroClient, accessor);
                default:
                    throw new NoProcessorDefinedForThisOrderTransferStatus();
            }
        }
    }
    public interface IOrderTransferProcessorStage {
        Task Handle(OrderTransfer orderTransfer);
    }

    public class NoProcessorDefinedForThisOrderTransferStatus : Exception { 
    
    }
}