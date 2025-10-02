using BirokratNext;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.tools.birokratops;
using core.tools.zalogaretriever;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_biro_to_woo {
    public class SimpleWoocommerceBiroToWoo : IBiroToWoo {

        IApiClientV2 client;
        IBirokratArtikelRetriever birokratArtikelRetriever;
        IOutApiClient wooclient;
        IBiroToWooProductSyncer syncer;
        BirokratField skuToBirokratField;

        public SimpleWoocommerceBiroToWoo(IApiClientV2 client,
            IOutApiClient wooclient,
            IBiroToWooProductSyncer syncer,
            IBirokratArtikelRetriever birokratArtikelRetriever,
            BirokratField skuToBirokratField) { // txtBarKoda
            this.client = client;
            this.birokratArtikelRetriever = birokratArtikelRetriever;
            this.wooclient = wooclient;
            this.syncer = syncer;
            this.skuToBirokratField = skuToBirokratField;
        }

        public BirokratField SkuBirokratField { get => skuToBirokratField; set => throw new NotImplementedException(); }
        public BirokratField VariableProductBirokratField { get => BirokratField.None; set => throw new NotImplementedException(); }

        public IBirokratArtikelRetriever GetBirokratArtikelRetriever() {
            return birokratArtikelRetriever;
        }

        public Dictionary<string, string> GetVariationAttributes() {
            return new Dictionary<string, string>();
        }

        public async Task OnArticleAdded(string sifra) {
            var biroArtikel = await birokratArtikelRetriever.Build(sifra);
            await syncer.AddProduct(biroArtikel);
        }

        public async Task OnArticleChanged(string sifra) {
            var biroArtikel = await birokratArtikelRetriever.Build(sifra);
            await syncer.UpdateProduct(biroArtikel);
        }

        public Task OnArticleDeleted(string sifra) {
            throw new NotImplementedException();
        }

        public void SetBirokratArtikelRetriever(IBirokratArtikelRetriever zaloga) {
            this.birokratArtikelRetriever = zaloga;
        }

    }
}
