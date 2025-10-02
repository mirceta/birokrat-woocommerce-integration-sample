using BirokratNext;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.tools.birokratops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tests.tests.hisavizij
{
    public class DefaultTestsetRetriever : IProductTestsetRetriever
    {

        int count;
        BirokratField varAttr;
        string varAttrName;
        public DefaultTestsetRetriever(BirokratField varAttr, int count = 20) {
            this.count = count;
            this.varAttr = varAttr;
            if (varAttr != BirokratField.None)
                varAttrName = BirokratNameOfFieldInFunctionality.KumulativaPodrobniPregledArtiklov(varAttr);
        }

        public async Task<List<string>> Get(IIntegration integ) {
            var tmp = await new PodrobniPregledArtiklov().GetPodrobniPregledArtiklov(integ.BiroClient);
            tmp = tmp.Where(x => (string)x["Prenesi v e-shop"] == "-1").ToList();
            if (tmp.Count == 0)
                throw new Exception("Cannot retrieve testset from birokrat that has no artikels internet artikels."); // should be TextException!!!

            List<string> sifre = new List<string>();
            if (varAttr != BirokratField.None)
            {
                sifre = handleVarAttrArtikels(tmp);
            }

            var simple = tmp
                    .Where(x => !isVarArtikel(x))
                    .Take(Math.Min(count, tmp.Count)).ToList();
            sifre.AddRange(simple.Select(x => (string)x["Artikel"]).ToList());

            return sifre;
        }

        private List<string> handleVarAttrArtikels(List<Dictionary<string, object>> tmp)
        {
            List<string> sifre;
            var variable = tmp
                    .Where(x => isVarArtikel(x))
                    .Take(Math.Min(count, tmp.Count)).ToList();
            sifre = variable.Select(x => (string)x["Artikel"]).ToList();
            return sifre;
        }

        private bool isVarArtikel(Dictionary<string, object> x)
        {
            return varAttr == BirokratField.None ? false : !string.IsNullOrEmpty((string)x[varAttrName]);
        }
    }

    public class SpicaTestsetRetriever : IProductTestsetRetriever
    {

        public SpicaTestsetRetriever() {
        }

        public async Task<List<string>> Get(IIntegration integ) {
            /*var tmp = await GBirokratOps.GetPodrobniPregledArtiklov(integration.BiroClient);
            tmp = tmp.Where(x => (string)x["Prenesi v e-shop"] == "-1").ToList();

            var variable = tmp
                    .Where(x => !string.IsNullOrEmpty((string)x["Barkoda 5"]))
                    .Where(x => ((string)x["Artikel"]).Contains(((string)x["Barkoda 5"])))
                    .ToList();
            var sifre = variable.Select(x => (string)x["Artikel"]).ToList();
            */
            var sifre = new List<string> { "70622-0444", "70622-0449", "70622-0158", "70622-0161", "3377", "11600914" };
            return sifre;
        }

    }

    public class KolosetTestsetRetriever : IProductTestsetRetriever
    {
        public async Task<List<string>> Get(IIntegration integ) {
            return new List<string> { "28302", "202100318", "04.0872", "1601", "1321", "30481754", "11600914" };
        }
    }

    public interface IProductTestsetRetriever {
        Task<List<string>> Get(IIntegration integ);
    }
}
