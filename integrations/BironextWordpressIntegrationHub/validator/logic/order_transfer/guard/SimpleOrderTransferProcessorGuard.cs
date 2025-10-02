using BironextWordpressIntegrationHub.structs;
using BiroWooHub.logic.integration;
using core.tools.wooops;
using System;
using System.Threading;
using System.Threading.Tasks;
using transfer_data_abstractions.orders;
using validator.logic.order_transfer.guard;

namespace validator.logic.order_transfer_processor
{
    public class SimpleOrderTransferProcessorGuard : IOrderTransferProcessorRoot {

        IOrderTransferProcessorStage next;
        IOrderTransferAccessor accessor;
        ISleepOperation sleeper;

        public SimpleOrderTransferProcessorGuard(
            IOrderTransferProcessorStage next,
            IOrderTransferAccessor accessor,
            ISleepOperation sleeper) {
            if (next == null)
                throw new ArgumentNullException("next");
            if (accessor == null)
                throw new ArgumentNullException("accessor");
            if (sleeper == null)
                throw new ArgumentNullException("sleeper");
            this.next = next;
            this.accessor = accessor;
            this.sleeper = sleeper;
        }

        public async Task HandleId(OrderTransfer orderTransfer) {

            var orderTransfer2 = await accessor.Get(orderTransfer.OrderId, orderTransfer.OrderStatus);
            if (orderTransfer2.OrderTransferStatus != orderTransfer.OrderTransferStatus) {
                // some other process has changed the order transfer state while we were waiting for the mutex, this one is then already processed!
                return;
            }

            if (orderTransfer.OrderTransferStatus == OrderTransferStatus.UNACCEPTED) {
                // sleep because controller may be processing UNACCEPTED at the moment
                // We assume that the processing will take less than 3 minutes
                // if it has done the processing, the its state will change, if it has not, then we are free to process it!
                Console.WriteLine("Now sleeping for 3 minutes for synchronization.");
                //await sleeper.Sleep();
            }

            orderTransfer2 = await accessor.Get(orderTransfer.OrderId, orderTransfer.OrderStatus);
            if (orderTransfer2.OrderTransferStatus != orderTransfer.OrderTransferStatus) {
                // some other process has changed the order transfer state while we were waiting for the mutex, this one is then already processed!
                return;
            }

            await next.Handle(orderTransfer);
        }

        public async Task HandleUnaccepted(string order) {

            if (string.IsNullOrEmpty(order)) {
                throw new ArgumentException("The input order cannot be null or empty");
            }
            var odr = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<WoocommerceOrder>(order);

            var orderTransfer = await accessor.Get(odr.Data.Id + "", odr.Data.Status);
            if (orderTransfer != null && orderTransfer.OrderTransferStatus != OrderTransferStatus.UNACCEPTED)
                return; // don't process orders that are in other states than unaccepted from controller - this must have been a status change to a status that has already been processed

            await next.Handle(new OrderTransfer() {
                OrderId = odr.Data.Id + "",
                OrderStatus = odr.Data.Status,
                OrderTransferStatus = OrderTransferStatus.UNACCEPTED,
                DateCreated = DateTime.Now
            });
        }
    }
}
