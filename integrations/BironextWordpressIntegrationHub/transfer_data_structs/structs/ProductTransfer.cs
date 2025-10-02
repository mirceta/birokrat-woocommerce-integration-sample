using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace tests_webshop.products
{
    public class ProductTransfer {

        public string product_id;
        public int last_event;
        public int last_event_success;
        public string last_event_message;
        public string last_event_datetime;

        [JsonConstructor]
        public ProductTransfer(string product_id,
            int last_event,
            int last_event_success,
            string last_event_message,
            string last_event_datetime) {
            this.product_id = product_id;
            this.last_event = last_event;
            this.last_event_success = last_event_success;
            this.last_event_message = last_event_message;
            this.last_event_datetime = last_event_datetime;
        }

        public ProductTransfer(string product_id,
            ProductTransferEvent last_event,
            ProductTransferSuccess last_event_success,
            string last_event_message,
            DateTime last_event_datetime) {
            this.product_id = product_id;
            this.last_event = getLastEvent(last_event);
            this.last_event_success = getSuccess(last_event_success);
            this.last_event_message = last_event_message;
            this.last_event_datetime = last_event_datetime.ToString("yyyy-MM-ddHH:mm:ss");
        }

        private int getLastEvent(ProductTransferEvent pevent) {
            Dictionary<ProductTransferEvent, int> map = new Dictionary<ProductTransferEvent, int>() {
                { ProductTransferEvent.ADD,  0},
                { ProductTransferEvent.SYNC, 1}
            };
            if (!map.ContainsKey(pevent)) {
                throw new Exception("Input ProductTransferEvent is not contained in the map!");
            }
            return map[pevent];
        }

        private int getSuccess(ProductTransferSuccess success) {
            Dictionary<ProductTransferSuccess, int> map = new Dictionary<ProductTransferSuccess, int>() {
                { ProductTransferSuccess.SUCCESSFUL,  0},
                { ProductTransferSuccess.BIROKRAT_ERROR, 1},
                { ProductTransferSuccess.INTEGRATION_ERROR, 2},
                { ProductTransferSuccess.INTERNAL_ERROR, 3},
            };
            if (!map.ContainsKey(success)) {
                throw new Exception("Input ProductTransferSuccess is not contained in the map!");
            }
            return map[success];
        }

        public override string ToString()
        {
            var objectToSerialize = new
            {
                product_id = this.product_id,
                last_event = ((ProductTransferEvent)this.last_event).ToString(),
                last_event_success = ((ProductTransferSuccess)this.last_event_success).ToString(),
                last_event_message = this.last_event_message,
                last_event_datetime = this.last_event_datetime
            };

            return JsonConvert.SerializeObject(objectToSerialize);
        }
    }

    public enum ProductTransferEvent
    {
        ADD,
        SYNC
    }

    public enum ProductTransferSuccess
    {
        SUCCESSFUL,
        BIROKRAT_ERROR,
        INTEGRATION_ERROR,
        INTERNAL_ERROR
    }
}
