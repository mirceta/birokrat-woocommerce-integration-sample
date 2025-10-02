using BironextWordpressIntegrationHub.structs;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace tests.tools
{
    public class TestUtils {

        public static WoocommerceOrder ModifyOrderForTesting(string json, int sessionId) {
            
            var order = JsonConvert.DeserializeObject<WoocommerceOrder>(json);


            // Hash the order data and assign to Id and Number because
            // 1. These values needs to be different than originals because originals are already added to database and we will get
            //    external identifier already exists error
            // 2. Any order always needs to get hashed to the same number so that we can compare resulting orders when we increase
            //    birokrat version etc.
            // - We append the sessionId so that when we put the pdfs that are controls to the same folder of the newly generated pdfs,
            //   those that originate from the same order will be put next to each other in the folder.
            // - We do the mods and multipliers exactly as it is to reduce probability of collisions and not overflow integer.
            var tmp = OrderNumHash(order);
            order.Data.Id = int.Parse(tmp + sessionId.ToString()) + order.Data.Id;
            order.Data.Number = tmp + "" + (sessionId % 100) + order.Data.Number;

            order.Data.DateCreated.Date = $"{DateTime.Now.Year}-{DateTime.Now.Month.ToString("00")}-{DateTime.Now.Day.ToString("00")} 11:01:22.000000";
            order.Data.DateModified.Date = $"{DateTime.Now.Year}-{DateTime.Now.Month.ToString("00")}-{DateTime.Now.Day.ToString("00")} 11:01:22.000000";
            order.Data.DateCompleted = $"{DateTime.Now.Year}-{DateTime.Now.Month.ToString("00")}-{DateTime.Now.Day.ToString("00")} 11:01:22.000000";
            order.Data.DatePaid = $"{DateTime.Now.Year}-{DateTime.Now.Month.ToString("00")}-{DateTime.Now.Day.ToString("00")} 11:01:22.000000";

            return order;
        }

        public static int OrderNumHash(WoocommerceOrder order) {
            var tmp = (int.Parse(NumHash(order.Data.Number)) % 10000) * 1000;
            tmp += (int.Parse(NumHash(order.Data.Id + "")) % 10000) * 100;
            tmp += (int.Parse(NumHash(order.Data.Billing.FirstName)) % 10000) * 10;
            tmp += (int.Parse(NumHash(order.Data.Billing.LastName)) % 10000);
            return tmp;
        }

        public static string NumHash(string go) {
            int sum = 0;
            for (int i = 0; i < go.Length; i++) {
                sum += go[i];
            }
            return sum + "";
        }
    }

    class TestFailedException : Exception {
        public TestFailedException(string message) : base(message) { 
        }
    }
}
