using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BirokratNext;
using BiroWoocommerceHubTests;
using BiroWoocommerceHubTests.tools;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.logic.mapping_biro_to_woo;
using core.tools.zalogaretriever;

namespace allintegrations_factories.wrappers
{
    class VariableBiroToWooBuilder
    {

        /*
         A builder class for the most common integration type:
            - simple and variable products
                - sku is $chosenfield
                - in simple products its straightforward
                - in var products -> $chosenbirokratvarfield goes into root sku, while $chosenfield goes into variations sku.
         */

        BirokratField skuField;
        BirokratField variableProductField;

        IApiClientV2 client; 
        IOutApiClient wooclient;
        List<IBirokratProductChangeHandler> changeHandlers;
        IBirokratArtikelRetriever birokratArtikelRetriever;
        VariationalAttributesBuilder variationalAttributesBuilder;
        bool zaloga;
        bool addOnFailToUpdate;

        public VariableBiroToWooBuilder(
            IApiClientV2 client,
            IOutApiClient wooclient,
            List<IBirokratProductChangeHandler> changeHandlers,
            IBirokratArtikelRetriever birokratArtikelRetriever,
            BirokratField skuField,
            BirokratField variableProductField,
            VariationalAttributesBuilder variationalAttributesBuilder,
            bool zaloga,
            bool addOnFailToUpdate)
        {
            this.skuField = skuField;
            this.variableProductField = variableProductField;
            this.client = client;
            this.wooclient = wooclient;
            this.changeHandlers = changeHandlers;
            this.birokratArtikelRetriever = birokratArtikelRetriever;
            this.variationalAttributesBuilder = variationalAttributesBuilder;
            this.zaloga = zaloga;
            this.addOnFailToUpdate = addOnFailToUpdate;
        }


        bool setupCalled = false;
        public async Task<VariableBiroToWooBuilder> Setup() {

            var simpleProductSyncer = new BiroToWooSimpleProductSyncer(wooclient,
            changeHandlers,
                await GetSimpleProductMapping(client, wooclient, variationalAttributesBuilder, zaloga),
                skuField,
                addOnFailToUpdate: addOnFailToUpdate);
            var tmp = new VariableProductSyncerBuilder(client, wooclient, changeHandlers,
                variableProductField,
                skuField,
                new BiroToWooTaxDefaults().taxdefaults(),
                zaloga: true,
                variationalAttributesBuilder,
                addOnFailToUpdate: addOnFailToUpdate);
            await tmp.Setup();
            var variableProductSyncer = tmp.GetSyncer();


            inner = new RegularWoocommerceBiroToWoo(client,
                wooclient,
                simpleProductSyncer,
                variableProductSyncer,
                birokratArtikelRetriever,
                variableProductField,
                skuField);

            setupCalled = true;
            return this;
        }

        IBiroToWoo inner;

        

        public IBiroToWoo Build()
        {
            if (!setupCalled)
                throw new System.Exception("VariableBiroToWooBuilder.Setup has not been called! You can't call Build!");
            return inner;
        }

        private async Task<ArtikelToProductMapping> GetSimpleProductMapping(IApiClientV2 client,
            IOutApiClient wooclient,
            VariationalAttributesBuilder variationalAttributesBuilder, bool zaloga)
        {
            var mapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                            .SetType(WooProductType.SIMPLE)
                            .SetZaloga(zaloga)
                            .SetTax(new BiroToWooTaxDefaults().taxdefaults())
                            .AddMapping(BirokratNameOfFieldInFunctionality.SifrantArtiklov(skuField), "sku")
                            .AddMapping("txtOpis", "name")
                            .AddMapping("txtAlternativniOpis", "description")
                            .AddMapping("PCsPD", "regular_price");
            mapping = await variationalAttributesBuilder.AppendAttributes(mapping);
            return (await ArtikelToProductMapping.NullObject()).SetCustom(mapping);
        }
    }
}
