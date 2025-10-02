using System;
using System.Collections.Generic;
using System.Text;
using transfer_data_abstractions.orders;

namespace validator.logic.order_transfer.retrying
{
    public class UnretriedErrosToUnaccepted {

        int retry_threshold;
        int days_back_threshold;
        PersistentRetryTracker retryTracker;

        public UnretriedErrosToUnaccepted(int days_back_threshold, int retry_threshold, PersistentRetryTracker tracker) {
            this.retry_threshold = retry_threshold;
            this.retryTracker = tracker;

            // we need to only retry for X days back, for example 5. If we make a mistake in this way, then the damage will not be big.
            // whereas without this we can risk trying to add 2 year old orders.
            this.days_back_threshold = days_back_threshold;
        }

        public OrderTransfer Set_UnretriedErrorOrderTransfers_toUnaccepted(string integration_name, OrderTransfer ot, IOrderTransferAccessor accessor) {
            if (DateTime.Now.Subtract(ot.DateCreated).Days > days_back_threshold)
                return ot; // don't change anything if order is too old
            
            if (ot.OrderTransferStatus == OrderTransferStatus.ERROR) {
                int numretries = retryTracker.Retries(integration_name, ot);
                if (numretries < 10) {
                    retryTracker.AddRetry(integration_name, ot);
                    ot.OrderTransferStatus = OrderTransferStatus.UNACCEPTED;
                    accessor.Set(ot);
                }
            }
            // Never reset
            return ot;
        }

    }
}
