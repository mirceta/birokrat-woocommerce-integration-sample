using ApiClient.utils;
using BiroWoocommerceHub.logic;
using core.customers.zgeneric;
using JsonIntegrationLoader.utils;
using tests.tools;

namespace allintegrations_factories.wrappers
{
    public class OrderFlowStageBuilderDecorator : IOrderFlowStageBuilder
    {
        private readonly IOrderFlowStageBuilder _next;

        public OrderFlowStageBuilderDecorator(IOrderFlowStageBuilder next)
        {
            _next = next;
        }

        public virtual OrderFlowStageBuilder AddStage(bool overrideSklicWithAdditionalNumber, OrderCondition orderCondition, BirokratDocumentType doctype, BirokratDocumentType sourceDocType, OrderAttributeTemplateParser2 template, bool fiscallize = false)
        {
            return _next.AddStage(overrideSklicWithAdditionalNumber, orderCondition, doctype, sourceDocType, template, fiscallize);
        }

        public virtual OrderFlowStageBuilder BuildOrderFlow()
        {
            return _next.BuildOrderFlow();
        }

        public virtual OrderFlow GetOrderFlow()
        {
            return _next.GetOrderFlow();
        }

        public virtual OrderFlowStageBuilder SetPartnerInserter(ISifrantPartnerjevInserter inserter)
        {
            return _next.SetPartnerInserter(inserter);
        }

        public virtual OrderFlowStageBuilder SetPostavkeOperations(bool includePostavkeComments, bool percentCoupons, bool fixedCartCoupons, bool includeShipping, bool includeHandlingOproscenaDobava)
        {
            return _next.SetPostavkeOperations(includePostavkeComments, percentCoupons, fixedCartCoupons, includeShipping, includeHandlingOproscenaDobava);
        }

        public virtual OrderFlowStageBuilder SetValidationComponents(ValidationComponents validationComponents)
        {
            return _next.SetValidationComponents(validationComponents);
        }
    }
}
