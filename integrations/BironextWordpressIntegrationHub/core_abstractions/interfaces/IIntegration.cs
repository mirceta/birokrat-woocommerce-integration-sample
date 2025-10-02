using BirokratNext;
using BiroWoocommerceHub;
using BiroWoocommerceHubTests;
using core.logic.common_birokrat;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.structs;
using gui_attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tests.tools;
using validator.logic.order_transfer.accessor;

namespace BiroWooHub.logic.integration
{
    public interface IIntegration {
        IApiClientV2 BiroClient { get; }

        // this should only have get;, but it was a lot easier to just set the outclient during product validation tests
        // where we had to inject sifras and then generate error cases for validator stage logic.
        IOutApiClient WooClient { get; set; }

        IBiroToWoo BiroToWoo { get; }
        IWooToBiro WooToBiro { get; }
        string WooToBiroIdentifier { get; }
        string BiroToWooIdentifier { get; }

        BirokratObvezneNastavitve ObvezneNastavitve { get; set; }


        bool IncludeValidator { get; }

        // obsolete - we will use more precise fields to identify an integration
        string Name { get; }

        PhpPluginConfig PhpPluginConfigVal { get; set; }
        string Datafolder { get; set; }

        IBirokratPostavkaExtractor WooToBiroPostavkaExtractor { get; set; }

        ValidationComponents ValidationComponents { get; set; }

        Options Options { get; }

        TestingConfiguration TestingConfiguration { get; set; }

        Dictionary<string, string> ExternalInfo { get; set; }
    }

    public class IntegrationIdentifier {

        public string WebshopName { get; set; }
        public string CompanyName { get; set; }
        public string IntegrationType { get; set; } // WOOTOBIRO, BIROTOWOO, VALIDATOR
        public string IntegrationName { get; set; } // PERHAPS WE WILL HAVE WOOTOBIRO1, WOOTOBIRO2 ETC ?

        public IntegrationIdentifier() { }


    }

    public class Options
    {


        private bool birotowoo_changetracker_dontincludearticlesthatarenotonwebshop = false;
        private bool birotowoo_use_shopify_product_retriever = false;
        private bool birotowoo_dont_include_validator = false;
        private OrderTransferSystemType orderTransferSystem = OrderTransferSystemType.DATA_WEBSHOP;

        public bool Birotowoo_changetracker_dontincludearticlesthatarenotonwebshop { get => birotowoo_changetracker_dontincludearticlesthatarenotonwebshop; set => birotowoo_changetracker_dontincludearticlesthatarenotonwebshop = value; }
        public bool Birotowoo_use_shopify_product_retriever { get => birotowoo_use_shopify_product_retriever; set => birotowoo_use_shopify_product_retriever = value; }
        public bool Birotowoo_dont_include_validator { get => birotowoo_dont_include_validator; set => birotowoo_dont_include_validator = value; }
        public OrderTransferSystemType OrderTransferSystem { get => orderTransferSystem; set => orderTransferSystem = value; }

        [GuiConstructor]
        public Options(bool birotowoo_changetracker_dontincludearticlesthatarenotonwebshop,
                       bool birotowoo_use_shopify_product_retriever,
                       bool birotowoo_dont_include_validator,
                       OrderTransferSystemType orderTransferSystem)
        {
            this.Birotowoo_changetracker_dontincludearticlesthatarenotonwebshop = birotowoo_changetracker_dontincludearticlesthatarenotonwebshop;
            this.Birotowoo_use_shopify_product_retriever = birotowoo_use_shopify_product_retriever;
            this.Birotowoo_dont_include_validator = birotowoo_dont_include_validator;
            this.OrderTransferSystem = orderTransferSystem;
        }

        public Options() { }
    }

    public enum OrderTransferSystemType
    {
        DATA_LOCAL, DATA_WEBSHOP
    }
}
