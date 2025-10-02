using BiroWoocommerceHubTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using validator;

namespace transfer_data.orders.sql_accessors.order_transfer_creator.deps
{
    public class OrderNotes_To_OrderTransfersAdapter
    {
        public OrderNotes_To_OrderTransfersAdapter()
        {
        }

        public async Task<List<OrderTransfer>> GetOrderStatusChangeHistory(List<StatusChange> orderStatusChanges, OrderStatusChangeInput order)
        {
            List<OrderTransfer> orderTransfers = new List<OrderTransfer>();
            orderStatusChanges = orderStatusChanges.OrderBy(x => x.EventDate).ToList();

            ValidateStatusTransitions(orderStatusChanges);

            orderTransfers.AddRange(CreateOrderTransfersFromStatusChanges(order.Id, orderStatusChanges, order.CreatedDate));

            if (!orderStatusChanges.Any())
            {
                orderTransfers.Add(CreateOrderTransferFromNoStatusChange(order));
            }

            return orderTransfers;
        }

        #region [auxiliary]
        private void ValidateStatusTransitions(List<StatusChange> orderedStatusChanges)
        {
            StatusChange previousChange = null;
            foreach (var statusChange in orderedStatusChanges)
            {
                if (previousChange != null && previousChange.To != statusChange.From)
                {
                    throw new Exception($"Status mismatch. Expected {previousChange.To} but found {statusChange.From}.");
                }
                previousChange = statusChange;
            }
        }

        private List<OrderTransfer> CreateOrderTransfersFromStatusChanges(string orderId, List<StatusChange> orderedStatusChanges, DateTime orderCreatedDate)
        {
            var transfers = orderedStatusChanges.Select(sc => new OrderTransfer
            {
                OrderId = orderId,
                OrderStatus = sc.To,
                DateCreated = sc.EventDate
            }).ToList();

            if (orderedStatusChanges.Any())
            {
                transfers.Add(new OrderTransfer
                {
                    OrderId = orderId,
                    OrderStatus = orderedStatusChanges.First().From,
                    DateCreated = orderCreatedDate
                });
            }

            return transfers;
        }

        private OrderTransfer CreateOrderTransferFromNoStatusChange(OrderStatusChangeInput order)
        {
            return new OrderTransfer
            {
                OrderId = order.Id.ToString(),
                OrderStatus = order.Status,
                DateCreated = order.CreatedDate // Adjust as needed
            };
        }
        #endregion
    }

    public class OrderStatusChangeInput
    { // id, status, date
        public string Id { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}