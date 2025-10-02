using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.tools.zalogaretriever;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace core.customers {
    class BiroToWoo : IBiroToWoo {

        public BiroToWoo() { 
        
        }

        public BirokratField SkuBirokratField { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public BirokratField VariableProductBirokratField { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IBirokratArtikelRetriever GetBirokratArtikelRetriever() {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetVariationAttributes() {
            throw new NotImplementedException();
        }

        public IZalogaRetriever GetZalogaRetriever() {
            throw new NotImplementedException();
        }

        public Task OnArticleAdded(string sifra) {
            throw new NotImplementedException();
        }

        public Task OnArticleChanged(string sifra) {
            throw new NotImplementedException();
        }

        public Task OnArticleDeleted(string sifra) {
            throw new NotImplementedException();
        }

        public void SetBirokratArtikelRetriever(IBirokratArtikelRetriever zaloga) {
            throw new NotImplementedException();
        }

        public void SetZalogaRetriever(IZalogaRetriever zaloga) {
            throw new NotImplementedException();
        }
    }
}
