using core.tools.wooops;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace tests_fixture
{
    internal class OrderDescriptionDeserializer {
        public static List<OrderDescription> Deserialize(string content) {
            var anon = new[] { new { id = 0, date_created = "", status = "" } };

            
            var list1 = JsonConvert.DeserializeAnonymousType(content, anon);
            var list = list1.Select(x => new OrderDescription()
            {
                date_created = x.date_created,
                status = x.status,
                id = GWooOps.SerializeIntWooProperty(x.id)
            }).ToList();
            
            list = list.Select(x =>
            {
                x.date_created = x.date_created.Substring(0, "yyyy-MM-dd".Length);
                return x;
            }).ToList();

            return list;
        }
    }

    internal class OrderDescription
    {
        public string id;
        public string date_created;
        public string status;
    }
}
