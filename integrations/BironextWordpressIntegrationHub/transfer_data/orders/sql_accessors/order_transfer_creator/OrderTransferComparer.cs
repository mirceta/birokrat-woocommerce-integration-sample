using System;
using System.Collections.Generic;
using validator;

namespace transfer_data.sql_accessors.order_transfer_creator
{
    public partial class WoocommerceOrderTransferCreator
    {
        internal class OrderTransferComparer : IEqualityComparer<OrderTransfer>
        {
            public bool Equals(OrderTransfer x, OrderTransfer y)
            {
                if (object.ReferenceEquals(x, y))
                    return true;

                if (x == null || y == null)
                    return false;

                return x.OrderId == y.OrderId && x.OrderStatus == y.OrderStatus;
            }

            public int GetHashCode(OrderTransfer obj)
            {
                  if (obj == null)
                    throw new ArgumentNullException(nameof(obj));

                return HashCode.Combine(obj.OrderId, obj.OrderStatus);
            }
        }

    }
}