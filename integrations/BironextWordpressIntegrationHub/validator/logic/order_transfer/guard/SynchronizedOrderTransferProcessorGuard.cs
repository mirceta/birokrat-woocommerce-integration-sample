using BironextWordpressIntegrationHub.structs;
using BiroWooHub.logic.integration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using transfer_data_abstractions.orders;

namespace validator.logic.order_transfer_processor
{

    public class SynchronizedOrderTransferProcessorGuard : IOrderTransferProcessorRoot {
        IOrderTransferProcessorStage next;
        OrderTransferLockFactory orderLockFactory;
        IIntegration integration;
        IOrderTransferAccessor accessor;

        public SynchronizedOrderTransferProcessorGuard(IOrderTransferProcessorStage next,
            IIntegration integration,
            OrderTransferLockFactory orderLockFactory,
            IOrderTransferAccessor accessor) {
            if (next == null)
                throw new Exception("Next OrderTransferProcessor cannot be null");
            if (integration == null)
                throw new Exception("Integration cannot be null");
            if (accessor == null)
                throw new Exception("Accessor cannot be null");
            if (orderLockFactory == null)
                throw new Exception("OrderLockFactory cannot be null");
            this.next = next;
            this.orderLockFactory = orderLockFactory;
            this.integration = integration;
            this.accessor = accessor;
        }

        public async Task HandleUnaccepted(string order) {

            var odr = JsonConvert.DeserializeObject<WoocommerceOrder>(order);

            var orderTransfer = accessor.Get(odr.Data.Id + "", odr.Data.Status);
            if (orderTransfer != null)
                return; // don't process orders that are already in ordertransfers table - this must have been a status change to a status that has already been processed


            var lck = orderLockFactory.Create(integration, odr.Data.Id + "", odr.Data.Status);
            lck.Lock();

            try {
                await next.Handle(new OrderTransfer() {
                    OrderId = odr.Data.Id + "",
                    OrderStatus = odr.Data.Status,
                    OrderTransferStatus = OrderTransferStatus.UNACCEPTED,
                    DateCreated = DateTime.Now
                });

            } catch (Exception ex) {
                lck.Unlock();
            } finally {
                lck.Unlock();
            }
        }

        public async Task HandleId(OrderTransfer orderTransfer) {
            
            var lck = orderLockFactory.Create(integration, orderTransfer.OrderId + "", orderTransfer.OrderStatus);
            lck.Lock();

            try {
                var orderTransfer2 = await accessor.Get(orderTransfer.OrderId, orderTransfer.OrderStatus);
                if (orderTransfer2.OrderTransferStatus != orderTransfer.OrderTransferStatus) {
                    // some other process has changed the order transfer state while we were waiting for the mutex, this one is then already processed!
                    return;
                }

                await next.Handle(orderTransfer);

            } catch (Exception ex) {
                lck.Unlock();
            } finally {
                lck.Unlock();
            }
        }

    }

    public class OrderTransferLockFactory {
        public IOrderLockState Create(IIntegration integration, string orderId, string orderStatus) {
            return new MutexOrderLockState(integration, orderId, orderStatus);
        }
        
    }

    public interface IOrderLockState {
        bool Lock();
        void Unlock();
    }

    public class MutexOrderLockState : IOrderLockState {

        IIntegration integration;
        string orderId;
        string orderStatus;

        Mutex mutex;

        public MutexOrderLockState(IIntegration integration, string orderId, string orderStatus) {
            this.integration = integration;
        }

        public bool Lock() {
            mutex = new Mutex(false, signature(integration, orderId, orderStatus));
            return mutex.WaitOne();
        }

        public void Unlock() {
            mutex.ReleaseMutex();
        }

        private static string signature(IIntegration integration, string orderId, string orderStatus) {
            
            return integration.Name + "_" + orderStatus + "_" + orderId;
        }
    }

    public class OrderToDocumentMapper { 
    
    }
}
