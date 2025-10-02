using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHubTests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using transfer_data.orders.sql_accessors;
using transfer_data.orders.sql_accessors.order_transfer_creator.deps;
using transfer_data.sql_accessors.order_transfer_creator.deps;
using transfer_data_abstractions.orders;
using validator;

namespace transfer_data.sql_accessors.order_transfer_creator
{

    public partial class WoocommerceOrderTransferCreator : IOrderTransferCreator
    {

        TimeSpan maxTimeWindow;
        DateTime neverLookBeforeDate;
        IOutApiClient outclient;

        OrderTransferDao otdao;
        int integId;
        int versionId;

        List<string> eventHooks;

        public WoocommerceOrderTransferCreator(TimeSpan timeWindow,
            DateTime neverLookBeforeDate,
            IOutApiClient client,
            OrderTransferDao otdao,
            int integId,
            int versionId,
            List<string> eventHooks)
        {

            maxTimeWindow = timeWindow;
            this.neverLookBeforeDate = neverLookBeforeDate;
            outclient = client;
            this.otdao = otdao;
            
            
            
            this.integId = integId;
            this.versionId = versionId;

            this.eventHooks = eventHooks;
        }


        public async Task CreateNewOrderTransfers()
        {
            await GetNewOrderTransfers_Then_InsertThemToDatabase();
        }

        private async Task GetNewOrderTransfers_Then_InsertThemToDatabase() {
            var newOrders_OrderTransfers = await GetAllInFirstLap();
            var oldOrders_NewOrderTransfers = await GetNewEventsInOldOrders();

            newOrders_OrderTransfers.AddRange(oldOrders_NewOrderTransfers);
            newOrders_OrderTransfers = newOrders_OrderTransfers.OrderBy(x => x.DateCreated).ToList();
            foreach (var x in newOrders_OrderTransfers)
            {
                try
                {
                    x.IntegrationId = integId;
                    x.VersionId = versionId;
                    await otdao.Insert(x);
                }
                catch
                {
                    Console.WriteLine();
                }
            }
        }

        private class OrderDescription {
            public string id;
            public string date_created;
            public string status;
        }

        async Task<List<OrderTransfer>> GetAllInFirstLap()
        {

            // we get the last order that arrived in order transfers, and we are sure
            // that all after it are not in the database yet
            var lastOrderInOrderTransfers = await otdao.GetDateOfLatestOrderInOrderTransfers(integId);
            if (lastOrderInOrderTransfers == null)
                lastOrderInOrderTransfers = DateTime.MinValue;
            lastOrderInOrderTransfers = findEarliestDatetimeToLook((DateTime)lastOrderInOrderTransfers);


            var orderDescs = (await outclient.GetOrderDescriptions(sinceDate: (DateTime)lastOrderInOrderTransfers));

            var ordersSinceLastOrderInOrderTransfers = orderDescs // date can be of the wrong timezone!
                        .Select(x => JsonConvert.DeserializeObject<OrderDescription>(x))
                        .Where(x => DateTime.Parse(x.date_created) > neverLookBeforeDate)
                        .ToList();

            return await adaptOrderDescriptionsToOrderTransfers(ordersSinceLastOrderInOrderTransfers.ToList());
        }

        async Task<List<OrderTransfer>> GetNewEventsInOldOrders()
        {

            // 2. {COVER OLD ONES FOR NEW EVENTS} get only order_notes for all orders that already have order transfers!

            DateTime dateThreshold = findEarliestDatetimeToLook(DateTime.MinValue);
            var timeWindowScopedOrderDescs = (await otdao
                .GetAllByIntegrationId(integId))
                .Where(x => x.DateCreated > dateThreshold)
                .ToList();


            var neededDescs = timeWindowScopedOrderDescs.Select(x => new OrderDescription()
                {
                    id = x.OrderId,
                    status = x.OrderStatus,
                    date_created = x.DateCreated.ToString("yyyy-MM-dd")
                })
                .GroupBy(x => x.id)
                .Select(x => x.First())
                .ToList();
            var azureOrderTransfers = await adaptOrderDescriptionsToOrderTransfers(neededDescs);


            var otcmp = new OrderTransferComparer();
            var newOrderTransfers = ComputeSetDifference(azureOrderTransfers, timeWindowScopedOrderDescs);


            return newOrderTransfers;
        }

        public static List<OrderTransfer> ComputeSetDifference(
            List<OrderTransfer> primaryList,
            List<OrderTransfer> subtractList)
        {
            var result = new List<OrderTransfer>();

            // Create a HashSet based on a concatenated key for O(1) lookups
            var subtractSet = new HashSet<string>(subtractList.Select(x => x.OrderId + "|" + x.OrderStatus));

            foreach (var primaryItem in primaryList)
            {
                string key = primaryItem.OrderId + "|" + primaryItem.OrderStatus;

                if (!subtractSet.Contains(key))
                {
                    result.Add(primaryItem);
                }
            }

            return result;
        }


        private async Task<List<OrderTransfer>> adaptOrderDescriptionsToOrderTransfers(List<OrderDescription> orders)
        {
            var ids = orders.Select(x => int.Parse(x.id)).ToList();
            var statusChanges = await outclient.GetOrderStatusChanges(ids);
            var orderTransfers = statusChanges.Zip(orders, async (statusChanges, order) =>
            {
                var new_ots = await new OrderNotes_To_OrderTransfersAdapter()
                                .GetOrderStatusChangeHistory(statusChanges, new OrderStatusChangeInput()
                                {
                                    Id = order.id + "",
                                    Status = order.status,
                                    CreatedDate = DateTime.Parse(order.date_created)
                                });
                new_ots = new_ots.Where(x => eventHooks.Contains(x.OrderStatus)).ToList();
                return new_ots;
            });

            var results = await Task.WhenAll(orderTransfers);
            var sm = results.Aggregate(new List<OrderTransfer>(), (x, y) => { x.AddRange(y); return x; });
            return sm;
        }

        private DateTime findEarliestDatetimeToLook(DateTime date) {
            DateTime dateThreshold = DateTime.Now.Subtract(maxTimeWindow);
            if (neverLookBeforeDate > dateThreshold)
                dateThreshold = neverLookBeforeDate;

            if (date < dateThreshold)
                return dateThreshold;
            return date;
        }

    }
}