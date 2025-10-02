using ApiClient.utils;
using BiroWoocommerceHub.logic;
using core.customers.zgeneric;
using JsonIntegrationLoader.utils;
using tests.tools;

namespace allintegrations_factories.wrappers
{
    public interface IOrderFlowStageBuilder
    {
        OrderFlowStageBuilder AddStage(bool overrideSklicWithAdditionalNumber, OrderCondition orderCondition, BirokratDocumentType doctype, BirokratDocumentType sourceDocType, OrderAttributeTemplateParser2 template, bool fiscallize = false);
        OrderFlowStageBuilder BuildOrderFlow();
        OrderFlow GetOrderFlow();
        OrderFlowStageBuilder SetPartnerInserter(ISifrantPartnerjevInserter inserter);
        OrderFlowStageBuilder SetPostavkeOperations(bool includePostavkeComments, bool percentCoupons, bool fixedCartCoupons, bool includeShipping, bool includeHandlingOproscenaDobava);
        OrderFlowStageBuilder SetValidationComponents(ValidationComponents validationComponents);
    }
}
