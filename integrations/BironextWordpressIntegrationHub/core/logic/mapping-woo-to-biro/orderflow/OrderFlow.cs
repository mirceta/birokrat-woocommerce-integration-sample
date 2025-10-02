using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub;
using BiroWoocommerceHub.logic;
using core.customers.poledancerka;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using validator.logic.order_transfer.accessor;
using BiroWoocommerceHubTests;
using gui_attributes;

namespace core.customers.zgeneric
{

    public class ConditionOperationPair
    {
        private OrderCondition orderCondition;
        private IOrderOperationCR orderOperation;

        public ConditionOperationPair(OrderCondition orderCondition, IOrderOperationCR orderOperation)
        {
            this.orderOperation = orderOperation;
            this.orderCondition = orderCondition;
        }

        public OrderCondition OrderCondition => orderCondition;
        public IOrderOperationCR OrderOperation => orderOperation;
    }

    public class ConditionAttachmentPair
    {
        private OrderCondition orderCondition;
        private IAttachmentOperationCR attachmentOperation;

        public ConditionAttachmentPair(OrderCondition orderCondition, IAttachmentOperationCR attachmentOperation)
        {
            this.attachmentOperation = attachmentOperation;
            this.orderCondition = orderCondition;
        }

        public OrderCondition OrderCondition => orderCondition;
        public IAttachmentOperationCR AttachmentOperation => attachmentOperation;
    }

    public class OrderFlow
    {

        List<ConditionOperationPair> orderFlow;
        List<ConditionAttachmentPair> attachmentFlow;
        IApiClientV2 client;
        ISifrantPartnerjevInserter partnerMapper;

        [GuiConstructor]
        public OrderFlow(IApiClientV2 client,
            ISifrantPartnerjevInserter partnerMapper,
            List<ConditionOperationPair> orderFlow,
            List<ConditionAttachmentPair> attachmentFlow)
        {

            if (client == null)
                throw new ArgumentNullException("client");
            if (partnerMapper == null)
                throw new ArgumentNullException("partnerMapper");
            this.client = client;
            this.partnerMapper = partnerMapper;

            this.orderFlow = orderFlow;
            this.attachmentFlow = attachmentFlow;
        }

        public OrderFlow(IApiClientV2 client,
            ISifrantPartnerjevInserter partnerMapper) {

            if (client == null)
                throw new ArgumentNullException("client");
            if (partnerMapper == null)
                throw new ArgumentNullException("partnerMapper");
            this.client = client;
            this.partnerMapper = partnerMapper;

            orderFlow = new List<ConditionOperationPair>();
            attachmentFlow = new List<ConditionAttachmentPair>();
        }

        public void AddOrderFlowStage(OrderCondition oc, IOrderOperationCR orderOps) {
            orderFlow.Add(new ConditionOperationPair(oc, orderOps));
        }

        public void AddAttachmentFlowStage(OrderCondition oc, IAttachmentOperationCR orderOps) {
            attachmentFlow.Add(new ConditionAttachmentPair(oc, orderOps));
        }

        public async Task<Dictionary<string, object>> OnOrderStatusChange(string body) {


            WoocommerceOrder order = null;
            try {
                order = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<WoocommerceOrder>(body);
            } catch (Exception ex) {

            }
            if (order.Data == null || order.Items == null) throw new Exception("Order deserialization error!");


            var ops = GetMatchingElements(order, orderFlow.Select(x => new KeyValuePair<OrderCondition, object>(x.OrderCondition, x.OrderOperation)).ToList());
            string oznaka = await partnerMapper.EnforceWoocommerceBillingPartnerCreated(order, null);
            
            foreach (IOrderOperationCR op in ops) {
                return await op.Next(order, new Dictionary<string, object>() {
                    { "partnerBirokratId", oznaka},
                    { "orderNumber", (string)order.Data.Number }
                }); // Warning! We actually don't support multiple order flows! just return first one!
            }
            return null;
        }

        public async Task<string> OnAttachmentRequest(string body) {

            WoocommerceOrder order = null;
            try {
                order = new JsonPowerDeserialization().DeserializeObjectImmuneToBadJSONEscapeSequenece<WoocommerceOrder>(body);
            } catch (Exception ex) {

            }

            var ops = GetMatchingElements(order, attachmentFlow.Select(x => new KeyValuePair<OrderCondition, object>(x.OrderCondition, x.AttachmentOperation)).ToList());
            return await ((IAttachmentOperationCR)ops[0]).Next(order, new Dictionary<string, object>());

        }


        private List<object> GetMatchingElements(WoocommerceOrder order, List<KeyValuePair<OrderCondition, object>> conditionEffectPairs) {


            string orderStatus = order.Data.Status;
            string orderPaymentMethod = order.Data.PaymentMethod;
            bool orderIsVatExempt = IsVatExempt(order);



            List<object> result = new List<object>();
            for (int i = 0; i < conditionEffectPairs.Count; i++) {
                OrderCondition oc = conditionEffectPairs[i].Key;

                List<string> status = oc.Status == null ? new List<string> { orderStatus } : oc.Status;
                bool isstatusmatch = status.Contains(orderStatus);


                bool isvatexemptmatch = (oc.IsVatExempt == "yes" && orderIsVatExempt) ||
                    (oc.IsVatExempt == "no" && !orderIsVatExempt) ||
                    string.IsNullOrEmpty(oc.IsVatExempt);


                bool ispaymentmethodmatch = false;
                if (!oc.NegatePaymentMethod)
                {
                    List<string> requiredToMatch_PaymentMethods = oc.PaymentMethod == null || oc.PaymentMethod.Count == 0 ? new List<string> { orderPaymentMethod } : oc.PaymentMethod;
                    ispaymentmethodmatch = requiredToMatch_PaymentMethods.Contains(orderPaymentMethod);
                }
                else {
                    ispaymentmethodmatch = !oc.PaymentMethod.Contains(orderPaymentMethod);
                }

                if (isstatusmatch && 
                    ispaymentmethodmatch && 
                    isvatexemptmatch) {

                    result.Add(conditionEffectPairs[i].Value);
                
                }


            }
            if (result.Count == 0)
                throw new OrderFlowOperationNotFoundException($"orderStatus: {orderStatus}, orderPaymentMethod: {orderPaymentMethod}");
            return result;
        }

        private static bool IsVatExempt(WoocommerceOrder order) {
            bool isvatexempt = false;
            var some = order.Data.MetaData.Where(x => x.Key == "is_vat_exempt").ToList();
            if (some.Count > 0) {
                string isVatExempt = Convert.ToString(some.First().Value);

                if (isVatExempt == "yes")
                    isvatexempt = true;
            }
            double sum = 0;
            foreach (var item in order.Items) {
                sum += double.Parse(item.SubtotalTax) + double.Parse(item.TotalTax);
            }
            if (sum == 0) {
                if (sum != 0 && isvatexempt == true) {
                }
                isvatexempt = true;
            }

            return isvatexempt;
        }
    }

    public class OrderFlowOperationNotFoundException : Exception
    {
        public OrderFlowOperationNotFoundException(string message) : base(message) { }
    }

    /*
     public class OrderFlow
    {

        List<KeyValuePair<OrderCondition, object>> orderFlow;
        List<KeyValuePair<OrderCondition, object>> attachmentFlow;
        IApiClientV2 client;
        ISifrantPartnerjevInserter partnerMapper;

        public OrderFlow(IApiClientV2 client,
            ISifrantPartnerjevInserter partnerMapper)
        {

            if (client == null)
                throw new ArgumentNullException("client");
            if (partnerMapper == null)
                throw new ArgumentNullException("partnerMapper");
            this.client = client;
            this.partnerMapper = partnerMapper;

            orderFlow = new List<KeyValuePair<OrderCondition, object>>();
            attachmentFlow = new List<KeyValuePair<OrderCondition, object>>();
        }

        public void AddOrderFlowStage(OrderCondition oc, IOrderOperationCR orderOps)
        {
            orderFlow.Add(KeyValuePair.Create(oc, (object)orderOps));
        }

        public void AddAttachmentFlowStage(OrderCondition oc, IAttachmentOperationCR orderOps)
        {
            attachmentFlow.Add(KeyValuePair.Create(oc, (object)orderOps));
        }

        OrderFlowHood orderFlowBuilt = null;


        public async Task<Dictionary<string, object>> OnOrderStatusChange(string body)
        {
            if (orderFlowBuilt == null)
                orderFlowBuilt = new OrderFlowHood(client, partnerMapper, orderFlow, attachmentFlow);
            return await orderFlowBuilt.OnOrderStatusChange(body);
        }

        public async Task<string> OnAttachmentRequest(string body)
        {
            if (orderFlowBuilt == null)
                orderFlowBuilt = new OrderFlowHood(client, partnerMapper, orderFlow, attachmentFlow);
            return await orderFlowBuilt.OnAttachmentRequest(body);
        }
    }*/
}
