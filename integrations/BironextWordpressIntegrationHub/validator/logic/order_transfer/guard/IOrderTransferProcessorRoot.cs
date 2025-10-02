using System.Threading.Tasks;

namespace validator.logic.order_transfer_processor {
    public interface IOrderTransferProcessorRoot {
        Task HandleUnaccepted(string order);
        Task HandleId(OrderTransfer orderTransfer);
    }
}
