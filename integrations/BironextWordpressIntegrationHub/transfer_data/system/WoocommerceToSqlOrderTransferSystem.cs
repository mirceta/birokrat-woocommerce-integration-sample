using BiroWooHub.logic.integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using transfer_data.orders.sql_accessors;
using transfer_data.sql_accessors;
using transfer_data.sql_accessors.order_transfer_creator;
using transfer_data_abstractions.orders;
using validator.logic.order_transfer.accessor;
using WooCommerceNET.WooCommerce.v2;

namespace transfer_data.system
{
    internal class WoocommerceToSqlOrderTransferSystem : IOrderTransferSystem
    {
        string connectionString;
        public WoocommerceToSqlOrderTransferSystem(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<IOrderTransferAccessor> GetOrderTransferAccessor(IIntegration integ)
        {
            safetyCheck(integ);

            int integrationId = int.Parse(integ.ExternalInfo["integrationId"]);

            return new SqlOrderTransferAccessor(connectionString, integrationId: integrationId, integ.WooClient);
        }

        public async Task<IOrderTransferCreator> GetOrderTransferCreator(IIntegration integ)
        {
            safetyCheck(integ);

            await transferOldOTSFromWebshopToSqlStore(integ);

            int integrationId = int.Parse(integ.ExternalInfo["integrationId"]);
            int versionId = int.Parse(integ.ExternalInfo["versionId"]);


            OrderTransferDao otdao = new OrderTransferDao(connectionString);

            // don't forget you need to inject integrationId AND integrationVersion into here!
            var creator = new WoocommerceOrderTransferCreator(
                new TimeSpan(15, 0, 0, 0),
                DateTime.Now.Subtract(new TimeSpan(90,0,0,0)),
                integ.WooClient,
                otdao,
                integId: integrationId,
                versionId: versionId,
                eventHooks: integ.TestingConfiguration.WooToBiro.TestedOrderStatusSequence); // having to refer to integ.TestingConfig. ... . . to get eventHooks is really weird..

            return creator;
        }

        public async Task transferOldOTSFromWebshopToSqlStore(IIntegration integ)
        {

            var y = await new WoocommerceToSqlOrderTransferSystem(connectionString).GetOrderTransferAccessor(integ);
            var some = await y.GetByStatus(null);
            if (some == null || some.Count == 0) // we do this only in this case!
            { // empty
                var x = new PureWoocommerceOrderTransferSystem();
                var orderTransfers = await (await x.GetOrderTransferAccessor(integ)).GetByStatus(null);
                foreach (var z in orderTransfers)
                {
                    z.IntegrationId = int.Parse(integ.ExternalInfo["integrationId"]);
                    z.VersionId = int.Parse(integ.ExternalInfo["versionId"]);
                    await ((SqlOrderTransferAccessor)y).DangerousInsert(z);
                }

                var inserted = await y.GetByStatus(null);

                int insertedCnt = inserted.Select(x => x.OrderId + x.OrderStatus)
                    .Intersect(orderTransfers.Select(x => x.OrderId + x.OrderStatus)).Count();

                if (insertedCnt != orderTransfers.Count)
                {
                    string err = "All of the orders have not been transfered from webshop to local sql database!";
                    err += "This is a requirement to be able to proceed with the program. Please inspect what went wrong.";
                    throw new System.Exception(err);
                }
            }
        }

        void safetyCheck(IIntegration integ) {
            if (integ.ExternalInfo.ContainsKey("integrationId") && integ.ExternalInfo.ContainsKey("versionId")) { }
            else
            {

                string msg = "Cannot create WoocommerceToSqlOrderTransferSystem because the injected integ object" +
                    " does not contain integrationId and versionId in its ExternalInfo property." +
                    " This implies that the source of the IIntegration object is from elsewhere than SQL.";
                throw new Exception(msg);
            }
        }
    }
}