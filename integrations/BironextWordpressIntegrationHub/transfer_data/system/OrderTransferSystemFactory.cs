using BiroWooHub.logic.integration;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_extractors;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Threading.Tasks;
using transfer_data.orders.sql_accessors;
using validator.logic.order_transfer.accessor;

namespace transfer_data.system
{
    public class OrderTransferSystemFactory
    {

        string connectionString;
        public OrderTransferSystemFactory(string connectionString = null)
        {
            this.connectionString = connectionString;
        }

        public IOrderTransferSystem Get(IIntegration integration)
        {

            IOrderTransferSystem orderTransferSystem = null;

            var option = integration.Options.OrderTransferSystem;
            if (option == OrderTransferSystemType.DATA_LOCAL)
            {
                return new WoocommerceToSqlOrderTransferSystem(connectionString);
            }
            else
            {
                return new PureWoocommerceOrderTransferSystem();
            }
        }
    }

    public class OrderTransferSystemForTransition_Pure_To_Sql {

        string connectionString;
        public OrderTransferSystemForTransition_Pure_To_Sql(string connectionString) { 
            this.connectionString = connectionString;
        }

        public async Task some(IIntegration integ) {
            
            var y = await new WoocommerceToSqlOrderTransferSystem(connectionString).GetOrderTransferAccessor(integ);
            var some = await y.GetByStatus(null);
            if (some == null || some.Count == 0)
            { // empty
                var x = new PureWoocommerceOrderTransferSystem();
                var orderTransfers = await (await x.GetOrderTransferAccessor(integ)).GetByStatus(null);
                foreach (var z in orderTransfers) {
                    await ((SqlOrderTransferAccessor)y).DangerousInsert(z);
                }

                var inserted = await y.GetByStatus(null);

                if (inserted.Intersect(orderTransfers).Count() != orderTransfers.Count) {
                    string err = "All of the orders have not been transfered from webshop to local sql database!";
                    err += "This is a requirement to be able to proceed with the program. Please inspect what went wrong.";
                    throw new System.Exception(err);
                }
            }
            else { 
                // proceed with using SqlOrderTransferSystem!
            }
        }
    }
}