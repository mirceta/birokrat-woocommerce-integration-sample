using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests.tests.estrada
{
    public interface IOrderStore {
        Task<List<string>> GetOrders();
    }
}
