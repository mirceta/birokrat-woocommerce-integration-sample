using BirokratNext;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.logic.mapping_biro_to_woo.syncers;
using core.structs;
using core.tools.zalogaretriever;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_biro_to_woo {
    public class GenericBiroToWoo : IBiroToWoo {
        public BirokratField SkuBirokratField { get => birokratPropName_of_variationProductSku; set => throw new NotImplementedException(); }
        public BirokratField VariableProductBirokratField { get => birokratPropName_of_baseVariationProductSku; set => throw new NotImplementedException(); }

        IApiClientV2 client;
        IOutApiClient wooclient;
        IBirokratArtikelRetriever biroArtikelRetriever;
        BiroToOutGenericSyncer genericSyncer;

        BirokratField birokratPropName_of_baseVariationProductSku;
        BirokratField birokratPropName_of_variationProductSku;

        public GenericBiroToWoo(IApiClientV2 client,
            IOutApiClient wooclient,
            BiroToOutGenericSyncer genericProductSyncer,
            IBirokratArtikelRetriever biroArtikelRetriever,
            BirokratField birokratPropName_of_baseVariationProductSku,
            BirokratField birokratPropName_of_variationProductSku) {
            this.client = client;
            this.wooclient = wooclient;
            this.biroArtikelRetriever = biroArtikelRetriever;
            this.genericSyncer = genericProductSyncer;
            this.birokratPropName_of_variationProductSku = birokratPropName_of_variationProductSku;
            this.birokratPropName_of_baseVariationProductSku = birokratPropName_of_baseVariationProductSku;
        }

        public IBirokratArtikelRetriever GetBirokratArtikelRetriever() {
            return biroArtikelRetriever;
        }

        public async Task OnArticleAdded(string sifra) {
            Dictionary<string, object> biroArtikel = await biroArtikelRetriever.Build(sifra);
            await genericSyncer.AddProduct(biroArtikel);
        }

        public async Task OnArticleChanged(string sifra) {
            Dictionary<string, object> biroArtikel = await biroArtikelRetriever.Build(sifra);
            await genericSyncer.UpdateProduct(biroArtikel);
        }

        public Task OnArticleDeleted(string sifra) {
            throw new NotImplementedException();
        }

        public void SetBirokratArtikelRetriever(IBirokratArtikelRetriever zaloga) {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetVariationAttributes() {
            return genericSyncer.GetAttributes();
        }
    }
}
