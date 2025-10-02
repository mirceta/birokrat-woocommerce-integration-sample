using biro_to_woo_common.executor.validation.validation_stages.validators.validation_operations;
using biro_to_woo_common.executor.validation_stages;
using biro_to_woo_common.executor.validation_stages.validators;
using biro_to_woo_common.executor.validation_stages.validators.validation_operations;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using Microsoft.VisualBasic;
using System.Collections.Generic;

namespace biro_to_woo_common.executor.validation.validation_stages.validators
{
    public class DatabaseAgreementComplianceVerifier
    {

        int everyXCalls = 0;
        public DatabaseAgreementComplianceVerifier(int everyXCalls = 0)
        {
            this.everyXCalls = everyXCalls;
        }

        public IBiroToOutValidationStage Get(IIntegration integration)
        {

            if (integration.BiroToWoo == null) {
                throw new System.Exception("This feature cannot work for integration objects that do not contain a BiroToWoo component. The input integration" +
                    "is setup to transfer orders and not synchronize, update and add products to the web store. Therefore this feature is not applicable to " +
                    "this integration.");
            }

            string variableProductField = null;
            if (integration.BiroToWoo.VariableProductBirokratField != BirokratField.None)
                variableProductField = BirokratNameOfFieldInFunctionality.SifrantArtiklov(integration.BiroToWoo.VariableProductBirokratField);

            var skuField = BirokratNameOfFieldInFunctionality.SifrantArtiklov(integration.BiroToWoo.SkuBirokratField);
            var allPossibleAttributes = integration.BiroToWoo.GetVariationAttributes();




            var flowstages = new List<IProductTransferVerifyOperation>();


            // only birokrat check - Barkoda5 nikoli ne sme biti ista kot sifra od kateregakoli produkta
            flowstages.Add(new Verify_That_Sifra_NotSameAsExistingValueForVariableAttribute(skuField, variableProductField));

            // first we deal with multiple products because after this we are sure we have no illegal
            // duplications in the set
            if (!integration.TestingConfiguration.BiroToWoo.allowMultipleProductsWithSameSkuOnWebshop)
            {
                flowstages.Add(new MoreThanOneProductOrVariationContainsSku(skuField));
            }
            else
            {
                flowstages.Add(new AllowSKUToBeInMultipleProducts_If_NoneOfTheseProductsIsADraft(skuField));
            }

            // then mismatches so we don't compare variable with simple products etc
            flowstages.Add(new ProductOutTypeMismatch(variableProductField, skuField));



            // finally only the things that pertain only to variable product attributes 
            if (variableProductField != null)
            {
                flowstages.Add(new BirokratVariableArtikelMissingAttributes(variableProductField, skuField, allPossibleAttributes));
                flowstages.Add(new ProductHasDifferentAttributesThanArticle(skuField, allPossibleAttributes));
                flowstages.Add(new RootOfVariationHasTheSameSifraAsVariableAttribute(skuField, variableProductField));
            }




            IBiroToOutValidationStage validation = new DatabaseAgreementComplianceVerifier_BTOStage(integration, flowstages);
            

            return validation;
        }
    }
}
