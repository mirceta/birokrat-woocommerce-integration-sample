using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHubTests;
using core.tools.wooops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace validator.logic {
    public class FixDateInOrder : IOrderPostprocessor {

        IOutApiClient wooclient;

        public FixDateInOrder(IOutApiClient wooclient) {
            this.wooclient = wooclient;
        }

        public WoocommerceOrder Postprocess(WoocommerceOrder order) {

            order.Data.DateCreated.Date = $"{DateTime.Now.Year}-{DateTime.Now.Month.ToString("00")}-{DateTime.Now.Day.ToString("00")} 11:01:22.000000";
            order.Data.DateModified.Date = $"{DateTime.Now.Year}-{DateTime.Now.Month.ToString("00")}-{DateTime.Now.Day.ToString("00")} 11:01:22.000000";
            order.Data.DateCompleted = $"{DateTime.Now.Year}-{DateTime.Now.Month.ToString("00")}-{DateTime.Now.Day.ToString("00")} 11:01:22.000000";
            order.Data.DatePaid = $"{DateTime.Now.Year}-{DateTime.Now.Month.ToString("00")}-{DateTime.Now.Day.ToString("00")} 11:01:22.000000";

            return order;
        }
    }
}