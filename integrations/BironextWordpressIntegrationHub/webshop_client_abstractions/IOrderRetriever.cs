 using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace validator.logic.order_transfer.accessor
{
    public interface IOrderRetriever
    {
        Task<string> GetOrder(string id);
    }
}
