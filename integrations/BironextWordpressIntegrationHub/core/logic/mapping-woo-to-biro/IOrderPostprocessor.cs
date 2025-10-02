using BironextWordpressIntegrationHub.structs;

namespace validator.logic
{
    public interface IOrderPostprocessor {
        WoocommerceOrder Postprocess(WoocommerceOrder order);
    }
}