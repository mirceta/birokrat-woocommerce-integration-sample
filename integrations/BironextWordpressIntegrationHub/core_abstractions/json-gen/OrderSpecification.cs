using BironextWordpressIntegrationHub.structs;

namespace BironextWordpressIntegrationHub {
    public class OrderSpecification {
        public OrderSpecification() { }
        public string order_date { get; set; }
        public string billing_city { get; set; }
        public Billing billing { get; set; }
        public Shipping shipping { get; set; }

    }

}
