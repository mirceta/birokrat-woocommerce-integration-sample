using System.Collections.Generic;
using System.Threading.Tasks;
using validator;

namespace transfer_data_abstractions.orders
{
    public interface IOrderTransferAccessor
    {
        Task Set(OrderTransfer orderTransfer);
        Task<List<OrderTransfer>> GetByStatus(List<OrderTransferStatus> statuses);
        Task<OrderTransfer> Get(string orderid, string orderstatus);
        Task<string> GetOrder(string id);
        Task AddUnaccepted(string orderid, string orderstatus);
        Task Delete(string orderid, string orderstatus);
    }
}