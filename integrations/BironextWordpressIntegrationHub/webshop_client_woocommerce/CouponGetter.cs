using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace webshop_client_woocommerce
{
    public class CouponGetter {

        WooApiClient client;
        DateTime lastFetched = DateTime.MinValue;
        int cacheTimeout;

        public CouponGetter(WooApiClient client, int cacheTimeout = 600) {
            this.client = client;
            this.coupons = new JArray();
            this.cacheTimeout = cacheTimeout;
        }

        JArray coupons;
        public async Task<JArray> Get()
        {
            if (DateTime.Now.Subtract(lastFetched).TotalSeconds > cacheTimeout)
            {
                await fetchCoupons();
            }
            return coupons;
        }

        private async Task fetchCoupons()
        {
            this.coupons.Clear();
            lastFetched = DateTime.Now;

            int perPage = 100; // Number of coupons to fetch per page
            int page = 1;
            bool hasMore = true;

            while (hasMore)
            {

                var response = await client.Get($"coupons?per_page={perPage}&page={page}");

                try
                {

                    JArray coupons = JArray.Parse(response);

                    foreach (var coupon in coupons)
                    {
                        this.coupons.Add(coupon);
                    }

                    if (coupons.Count < perPage)
                    {
                        hasMore = false;
                    }
                    else
                    {
                        page++;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Coupons couldn't be retrieved: " + response);
                }
            }

            
        }
    }

    public class VariationGetter
    {

        WooApiClient client;
        string productId;

        public VariationGetter(WooApiClient client, string productId)
        {
            this.client = client;
            this.coupons = new JArray();
            this.productId = productId;
        }

        JArray coupons;
        public async Task<JArray> Get()
        {
            await fetchVariations();
            return coupons;
        }

        private async Task fetchVariations()
        {
            this.coupons.Clear();

            int perPage = 100; // Number of coupons to fetch per page
            int page = 1;
            bool hasMore = true;

            while (hasMore)
            {

                var response = await client.Get($"products/{productId}/variations?per_page={perPage}&page={page}");

                try
                {

                    JArray coupons = JArray.Parse(response);

                    foreach (var coupon in coupons)
                    {
                        this.coupons.Add(coupon);
                    }

                    if (coupons.Count < perPage)
                    {
                        hasMore = false;
                    }
                    else
                    {
                        page++;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Product variations couldn't be retrieved: " + response);
                }
            }


        }
    }
}
