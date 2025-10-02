using System.Collections.Generic;
using System.Threading.Tasks;
using allintegrations.customers;
using allintegrations_factories;
using allintegrations_factories.wrappers;
using ApiClient.utils;
using BirokratNext;
using BiroWoocommerceHub;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.customers.spicasport;
using core.customers.zgeneric;
using core.logic.common_birokrat;
using core.logic.common_woo;
using core.structs;
using core.tools.zalogaretriever;
using JsonIntegrationLoader.utils;
using tests.tools;

namespace allintegrations_factories.customers.NOVE
{
    public class BogartIntegrationFactory
    {

        bool debug;
        string datafolder;

        VariationalAttributesBuilder variationalAttributesBuilder;
        ValidationComponents validationComponents;

        public BogartIntegrationFactory(bool debug, string datafolder)
        {
            this.debug = debug;
            this.datafolder = datafolder;

            variationalAttributesBuilder = new VariationalAttributesBuilder();
        }


        public async Task<IIntegration> BuildIntegration(bool wootobiro, IApiClientV2 client, IOutApiClient wooclient,
            string biroApiKey, string name, IntegrationIdentifier identifier)
        {
            validationComponents = new ValidationComponents(
                    new HardcodedCountryMapper(),
                    new VatNumberParser(),
                    variationalAttributesBuilder.GetTestEqualAdditions(),
                    null);


            IWooToBiro wootobiroi = null;
            if (wootobiro)
                wootobiroi = GetWooToBiro(client, wooclient);



            var integ = new RegularIntegration(client, wooclient, biroApiKey, name, identifier,
                new BirokratObvezneNastavitve(new Dictionary<string, string>()), true, validationComponents
                )
                            .SetPhpConfig(new PhpPluginConfig()
                            {
                                ProductHooks = false,
                                AcceptableAttachmentOrderStatuses = new List<string>() { },
                                AttachmentHook = true,
                                OrderStatusHooks = new List<string>() { "on-hold", "processing" }
                            })
                            .SetWooToBiro(wootobiroi)
                            .SetBiroToWoo(null)
                            .SetDatafolder(datafolder);

            integ.TestingConfiguration = TestingConfigGenHelper.GetTestingConfiguration(integ,
                null,
                orderStatuses: new List<string>() { "on-hold", "processing" });
            //integ.Options["birotowoo_changetracker_dontincludearticlesthatarenotonwebshop"] = "true";

            return integ;
        }

        public IWooToBiro GetWooToBiro(IApiClientV2 client, IOutApiClient wooclient)
        {
            var builder = new OrderFlowStageBuilder(debug, client,
                new HardcodedCountryMapper(),
                datafolder);

            builder.SetValidationComponents(validationComponents);

            builder.SetPostavkeOperations(includePostavkeComments: true,
                percentCoupons: true,
                fixedCartCoupons: true,
                includeShipping: true,
                includeHandlingOproscenaDobava: false);
            builder.BuildOrderFlow();

            var docNumTemplate = new OrderAttributeTemplateParser2(
                    "1-$$$PAYMENT_METHOD$$$-$$$ORDER_NUMBER$$$",
                    new OrderAttributeTemplateParserDecorator(
                        new Dictionary<string, string>() {
                            { "cod", "1" },
                            { "bacs", "2" },
                            { "ppcp-gateway", "3"}
                        })
                    );

            builder.AddStage(
                overrideSklicWithAdditionalNumber: true,
                orderCondition: new OrderCondition()
                {
                    Status = new List<string>() { "on-hold", "processing" },
                    PaymentMethod = null
                },
                doctype: BirokratDocumentType.RACUN,
                sourceDocType: BirokratDocumentType.UNASSIGNED,
                parser: docNumTemplate
            );

            var orderflow = builder.GetOrderFlow();
            return new OrderFlowProductInserterWooToBiro(orderflow, null);
        }
    }
}
