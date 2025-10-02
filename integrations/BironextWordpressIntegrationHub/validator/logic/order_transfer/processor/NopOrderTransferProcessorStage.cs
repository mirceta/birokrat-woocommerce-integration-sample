using System.Threading.Tasks;

namespace validator.logic {
    public class NopOrderTransferProcessorStage : IOrderTransferProcessorStage
    {
        public async Task Handle(OrderTransfer orderTransfer) {
        }
    }
}