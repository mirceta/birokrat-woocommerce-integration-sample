using BiroWooHub.logic.integration;

namespace transfer_data_abstractions.orders
{
    public interface IOrderTransferAccessorFactory
    {
        IOrderTransferAccessor Create(IIntegration integration);
    }
}
