using BironextWordpressIntegrationHub.structs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.customers.zgeneric {
    public interface IOrderOperationCR {
        public Task<Dictionary<string, object>> Next(WoocommerceOrder order, Dictionary<string, object> data);
    }

    public interface IAttachmentOperationCR {
        public Task<string> Next(WoocommerceOrder order, Dictionary<string, object> data);
    }
}
