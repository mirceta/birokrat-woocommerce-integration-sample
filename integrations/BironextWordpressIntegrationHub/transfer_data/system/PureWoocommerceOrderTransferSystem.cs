using BiroWooHub.logic.integration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Threading.Tasks;
using transfer_data.orders.accessor;
using transfer_data_abstractions.orders;
using validator.logic.order_transfer.accessor;

namespace transfer_data.system
{
    public class PureWoocommerceOrderTransferSystem : IOrderTransferSystem
    {

        public PureWoocommerceOrderTransferSystem() { }

        public async Task<IOrderTransferAccessor> GetOrderTransferAccessor(IIntegration integ)
        {
            return new WoocommerceWebshopOrderTransferAccessor(integ.WooClient);
        }

        public async Task<IOrderTransferCreator> GetOrderTransferCreator(IIntegration integ)
        {
            return new NullOrderTransferCreator();
        }
    }
}