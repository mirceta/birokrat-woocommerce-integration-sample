using BiroWoocommerceHubTests;
using Newtonsoft.Json.Linq;
using order_mapping;
using System;
using System.Threading.Tasks;
using webshop_client_woocommerce;

namespace validator.logic.order_transfer.accessor
{
    public class WooNativeRestApiOrderRetriever : IOrderRetriever
    {

        IOutApiClient wooclient;
        CouponGetter couponGetter;

        public WooNativeRestApiOrderRetriever(IOutApiClient wooclient, CouponGetter couponGetter)
        {
            if (wooclient == null)
                throw new ArgumentNullException("wooclient");
            if (couponGetter == null)
                throw new ArgumentNullException("couponGetter");
            this.wooclient = wooclient;
            this.couponGetter = couponGetter;
        }

        public async Task<string> GetOrder(string id)
        {

            var some = await wooclient.Get($"orders/{id}");


            var transformer = new WoocommerceOrderFormatTransformer(couponGetter);

            var obj1 = JObject.Parse(some);
            obj1 = await transformer.TransformStageOne_InternetDependent(wooclient, obj1);

            obj1 = await transformer.TransformStageTwo_InternetIndependent(obj1);
            string outputJson = obj1.ToString();

            return outputJson;


        }
    }
}
