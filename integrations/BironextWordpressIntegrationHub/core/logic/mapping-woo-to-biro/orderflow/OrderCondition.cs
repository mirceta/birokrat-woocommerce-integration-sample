using System.Collections.Generic;
using gui_attributes;
using gui_inferable;

namespace core.customers.zgeneric
{
    public class OrderCondition : IInferable
    {
        private List<string> status;
        private List<string> paymentMethod;
        private bool negatePaymentMethod;
        private string isVatExempt;

        public OrderCondition() { }

        [GuiConstructor]
        public OrderCondition(List<string> status, List<string> paymentMethod, bool negatePaymentMethod, string isVatExempt)
        {
            this.status = status ?? new List<string>();
            this.paymentMethod = paymentMethod ?? new List<string>();
            this.negatePaymentMethod = negatePaymentMethod;
            this.isVatExempt = isVatExempt;
        }

        public List<string> Status
        {
            get => status;
            set => status = value ?? new List<string>();
        }

        public List<string> PaymentMethod
        {
            get => paymentMethod;
            set => paymentMethod = value ?? new List<string>();
        }

        public bool NegatePaymentMethod
        {
            get => negatePaymentMethod;
            set => negatePaymentMethod = value;
        }

        public string IsVatExempt
        {
            get => isVatExempt;
            set => isVatExempt = value;
        }

        public Dictionary<string, object> Infer(Dictionary<string, object> state)
        {
            var statusList = new List<string>();
            statusList.AddRange(status);

            state["statusList"] = statusList;

            
            
            return state;
        }
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
