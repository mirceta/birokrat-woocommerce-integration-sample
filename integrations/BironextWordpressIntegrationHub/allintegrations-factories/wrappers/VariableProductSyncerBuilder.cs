using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BirokratNext;
using BiroWoocommerceHubTests;
using BiroWoocommerceHubTests.tools;
using core.logic.common_birokrat;
using core.logic.mapping_biro_to_woo;
using core.tools.attributemapper;

namespace allintegrations_factories.wrappers
{
    class VariableProductSyncerBuilder
    {


        BiroToWooVariableProductSyncer syncer;


        IApiClientV2 client;
        IOutApiClient wooclient;
        List<IBirokratProductChangeHandler> changeHandlers;
        BirokratField birokratVariableProductField;
        BirokratField birokratSkuField;
        BiroTaxToWooTax tax;
        bool zaloga;
        VariationalAttributesBuilder attrBuilder;
        bool addOnFailToUpdate;

        public VariableProductSyncerBuilder(IApiClientV2 client, 
            IOutApiClient wooclient,
            List<IBirokratProductChangeHandler> changeHandlers,
            BirokratField birokratVariableProductField,
            BirokratField birokratSkuField,
            BiroTaxToWooTax tax,
            bool zaloga,
            VariationalAttributesBuilder attrBuilder,
            bool addOnFailToUpdate)
        {
            this.client = client;
            this.wooclient = wooclient;
            this.changeHandlers = changeHandlers;
            this.birokratVariableProductField = birokratVariableProductField;
            this.birokratSkuField = birokratSkuField;
            this.tax = tax;
            this.zaloga = zaloga;
            this.attrBuilder = attrBuilder;
            this.addOnFailToUpdate = addOnFailToUpdate;
        }


        bool setupCalled = false;
        public async Task Setup() {
            var mapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                                .SetType(WooProductType.VARIABLE)
                                .SetZaloga(false) // ta je samo osnovni
                                .SetTax(tax)
                                .AddMapping(BirokratNameOfFieldInFunctionality.SifrantArtiklov(birokratVariableProductField), "sku")
                                .AddMapping("txtOpis", "name")
                                .AddMapping("txtAlternativniOpis", "description")
                                .AddMapping("PCsPD", "regular_price");
            mapping = await attrBuilder.AppendAttributes(mapping);



            var varmapping = new BirokratArtikelToWooProductMapping(client, wooclient)
                            .SetZaloga(zaloga)
                            .AddMapping(BirokratNameOfFieldInFunctionality.SifrantArtiklov(birokratSkuField), "sku")
                            .AddMapping("PCsPD", "regular_price");
            varmapping = await attrBuilder.AppendAttributes(varmapping);

            syncer = new BiroToWooVariableProductSyncer(wooclient,
                changeHandlers,
                (await ArtikelToProductMapping.NullObject()).SetCustom(mapping),
                (await ArtikelToProductMapping.NullObject()).SetCustom(varmapping),
                birokratVariableProductField,
                birokratSkuField,
                addOnFailToUpdate: addOnFailToUpdate,
                requireBaseSkuAttrPrefixOfVariationSkuAttr: false);

            setupCalled = true;
        }

        public BiroToWooVariableProductSyncer GetSyncer() {
            if (!setupCalled)
                throw new System.Exception("Setup has not been called in VariableProductSyncerBuilder!");
            return syncer; 
        }
    }
}
