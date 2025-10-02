using System.Threading.Tasks;

namespace transfer_data_abstractions.orders
{
    public interface IOrderTransferCreator
    {
        Task CreateNewOrderTransfers();
    }

    public class NullOrderTransferCreator : IOrderTransferCreator
    {
        public Task CreateNewOrderTransfers()
        {
            return Task.CompletedTask;
        }
    }
}
