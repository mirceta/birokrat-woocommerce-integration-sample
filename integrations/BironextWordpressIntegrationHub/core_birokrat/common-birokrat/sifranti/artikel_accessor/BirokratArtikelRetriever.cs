using BirokratNext;
using core.logic.common_birokrat;
using core.tools.birokratops;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace core.tools.zalogaretriever {
    public class BirokratArtikelRetriever : IBirokratArtikelRetriever {

        // WILL RETURN THE ARTICLES WITH DICTIONARY KEYS THAT ALWAYS FIT SIFRANT ARTIKLOV PL NAMES

        IApiClientV2 client;
        IZalogaRetriever zalogaRetriever;

        public BirokratArtikelRetriever(IApiClientV2 client, IZalogaRetriever zalogaRetriever) {
            this.client = client;
            this.zalogaRetriever = zalogaRetriever;
        }

        public async Task<Dictionary<string, object>> Build(string sifra) {
            var birokratObj = await GBirokratOps.GetAndBuildBirokratArtikel(client, zalogaRetriever, sifra);
            return birokratObj;
        }

        public async Task<List<Dictionary<string, object>>> Query(Dictionary<string, object> additionalAttributes, Dictionary<string, object> parameters) {

            var podrobniPregled = new core.logic.common_birokrat.PodrobniPregledArtiklov();
            if (additionalAttributes != null)
                podrobniPregled.AddAdditionalParameters(additionalAttributes);
            if (parameters != null)
                podrobniPregled.ModifyParameters(parameters);
            var lstPodrobniPregled = await podrobniPregled.GetPodrobniPregledArtiklov(client);


            List<Dictionary<string, object>> lstResultItems = new List<Dictionary<string, object>>();
            AddBasicFieldsToAllRecords(lstPodrobniPregled, lstResultItems);


            if (additionalAttributes != null)
                AddAdditionalAttributeFields(additionalAttributes, lstPodrobniPregled, lstResultItems);

            if (zalogaRetriever != null)
                await AddZalogaFieldToAllRecords(lstResultItems);

            return lstResultItems;
        }

        private void AddBasicFieldsToAllRecords(List<Dictionary<string, object>> result, List<Dictionary<string, object>> lstReturn) {
            foreach (var res in result) {

                Dictionary<string, object> tmp = new Dictionary<string, object>();

                AddFieldFromPodrobniPregled(BirokratField.SifraArtikla, res, tmp);
                AddFieldFromPodrobniPregled(BirokratField.Barkoda, res, tmp);
                AddFieldFromPodrobniPregled(BirokratField.Barkoda2, res, tmp);
                AddFieldFromPodrobniPregled(BirokratField.Barkoda3, res, tmp);
                AddFieldFromPodrobniPregled(BirokratField.Barkoda4, res, tmp);
                AddFieldFromPodrobniPregled(BirokratField.Barkoda5, res, tmp);
                AddFieldFromPodrobniPregled(BirokratField.PCsPD, res, tmp);
                AddFieldFromPodrobniPregled(BirokratField.Internet, res, tmp);

                lstReturn.Add(tmp);
            }
        }

        private static void AddAdditionalAttributeFields(Dictionary<string, object> additionalAttributes,
            List<Dictionary<string, object>> lstPodrobniPregled,
            List<Dictionary<string, object>> lstReturn) {

            if (additionalAttributes == null)
                return;

            lstReturn.Zip(lstPodrobniPregled, (res, orig) => {
                foreach (var x in additionalAttributes) {
                    res[x.Key] = orig[x.Key];
                }
                return 0;
            }).ToList();
        }

        private async Task AddZalogaFieldToAllRecords(List<Dictionary<string, object>> lstReturn) {
            var zaloga = await zalogaRetriever.Query();

            foreach (var x in lstReturn) {
                x["zaloga"] = "0"; // so that all at least have the key!
            }
            foreach (var entry in zaloga) {
                string sifra = entry.Item1;
                string zal = entry.Item2;
                foreach (var x in lstReturn) {

                    string sifrafield = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla);

                    if (((string)x[sifrafield]).Trim() == sifra) {
                        x["zaloga"] = zal;
                        break;
                    }
                }
            }
        }

        void AddFieldFromPodrobniPregled(BirokratField field, Dictionary<string, object> old, Dictionary<string, object> nw) {
            string kumsifra = BirokratNameOfFieldInFunctionality.KumulativaPodrobniPregledArtiklov(field);
            string sifsifra = BirokratNameOfFieldInFunctionality.SifrantArtiklov(field);
            nw[sifsifra] = ((string)old[kumsifra]).Trim();
        }
    }
}
