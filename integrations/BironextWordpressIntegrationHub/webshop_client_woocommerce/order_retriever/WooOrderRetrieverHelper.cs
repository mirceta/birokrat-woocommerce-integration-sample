using BiroWoocommerceHubTests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace tests_fixture
{
    public class WooOrderRetrieverHelper {


        int maxDaysOld = 1800;
        IOutApiClient wooclient;
        int maxCount = 500000;
        public WooOrderRetrieverHelper(int maxDaysOld, IOutApiClient wooclient) {
            this.maxDaysOld = maxDaysOld;
            this.wooclient = wooclient;
        }

        public WooOrderRetrieverHelper(int maxDaysOld, int maxCount, IOutApiClient wooclient) {
            this.maxDaysOld = maxDaysOld;
            this.wooclient = wooclient;
            this.maxCount = maxCount;
        }

        public List<string> GetFullOrders() {
            var factory = new OrderRetrieverModeFactory(null, "ids");
            WooOrderRetriever retriever = new WooOrderRetriever(factory, 5);
            retriever.SetMaxDaysOld(maxDaysOld);
            retriever.SetMaxCount(maxCount);
            List<string> fullOrders = retriever.Get(wooclient).Select(x => JsonConvert.SerializeObject(x)).ToList();
            return fullOrders;
        }

        public List<string> GetOrderIds()
        {
            var factory = new OrderRetrieverModeFactory(null, "ids");
            WooOrderRetriever retriever = new WooOrderRetriever(factory, 5);
            retriever.SetMaxDaysOld(maxDaysOld);
            retriever.SetMaxCount(maxCount);
            List<string> webshopIds = retriever.Get(wooclient).Select(x => JsonConvert.SerializeObject(x)).ToList();

            var anon = new { id = "", date_created = "" };

            // filter too old ones
            webshopIds = webshopIds.Where(x => DateTime.Now.Subtract(new TimeSpan(maxDaysOld, 0, 0, 0)) < DateTime.ParseExact(
                JsonConvert.DeserializeAnonymousType(x, anon).date_created, "yyyy-MM-dd", CultureInfo.InvariantCulture))
                .Select(x => JsonConvert.DeserializeAnonymousType(x, anon).id).ToList();

            return webshopIds;
        }
    }
}
