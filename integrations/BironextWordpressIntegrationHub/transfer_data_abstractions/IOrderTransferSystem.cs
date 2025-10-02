using BiroWooHub.logic.integration;
using System.Threading.Tasks;
using transfer_data_abstractions.orders;

namespace validator.logic.order_transfer.accessor
{
    public interface IOrderTransferSystem {
        Task<IOrderTransferCreator> GetOrderTransferCreator(IIntegration integ);
        Task<IOrderTransferAccessor> GetOrderTransferAccessor(IIntegration integ);
        
    }
}
