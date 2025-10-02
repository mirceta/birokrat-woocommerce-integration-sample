using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using core.logic.common_woo;
using core.tools.wooops;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic
{

    public class ClassicPartnerInserter : ISifrantPartnerjevInserter {

        const string SIFRANTPARTNERJEVPATH = @"sifranti\poslovnipartnerjiinosebe\poslovnipartnerji";

        IApiClientV2 client;
        IPartnerWooToBiroMapper attributeMapper;
        IVatIdParser vatIdParser;
        IWooToBiroPartnerSearcher partnerSearcher;
        bool povoziVsePartnerjeveAtribute;

        public ClassicPartnerInserter(IApiClientV2 client,
            IPartnerWooToBiroMapper attributeMapper,
            IVatIdParser vatIdParser,
            bool povoziVsePartnerjeveAtribute = false) {
            
            this.client = client;
            this.povoziVsePartnerjeveAtribute = povoziVsePartnerjeveAtribute;
            this.attributeMapper = attributeMapper;
            
            if (vatIdParser == null) {
                this.vatIdParser = new VatNumberParser(); // default plugin
            } else {
                this.vatIdParser = vatIdParser;
            }

            partnerSearcher = new NazivUlicaSearcher(attributeMapper, client);
        }
         
        public async Task<string> EnforceWoocommerceBillingPartnerCreated(WoocommerceOrder order, Dictionary<string, string> additionalInfo) {

            string woodavcna = await vatIdParser.Get(order);

            var match = await partnerSearcher.MatchWooToBiroUser(order, additionalInfo);

            if (match == null) {
                return await UstvariNovegaPartnerja(order, woodavcna);
            }

            string oznaka = await Bullshit(order, woodavcna, match);

            return oznaka;
        }

        private async Task<string> Bullshit(WoocommerceOrder order, string woodavcna, Dictionary<string, object> match) {
            string oznaka = (string)match["Oznaka"];

            if (PartnerObstajaInNimaDavcne(match) && !string.IsNullOrEmpty(woodavcna)) {
                // TO SPLOH NI CASE - TEGA NIKOL NE MATCHAMO!
                await DodajDavcnoVObstojecegaPartnerja(woodavcna, oznaka);
                return oznaka;
            } else {
                string partnerDavcna = GetPartnerDavcna(match);
                if (!string.IsNullOrEmpty(partnerDavcna) &&
                    !string.IsNullOrEmpty(woodavcna) &&
                    partnerDavcna != woodavcna) {
                    // davcni nista null in sta drugacni, torej je invaliden match - treba je narest novega partnerja!
                    // TO TUD SPLOH NI CASE
                    oznaka = await UstvariNovegaPartnerja(order, woodavcna);
                } else {
                    if (povoziVsePartnerjeveAtribute) {
                        await PovoziObstojecegaPartnerja(order, oznaka);
                    }
                }
            }
            return oznaka;
        }

        private static bool PartnerObstajaInNimaDavcne(Dictionary<string, object> match) {
            return match != null && match.ContainsKey("Davčna številka") &&
                                string.IsNullOrEmpty((string)match["Davčna številka"]);
        }

        private static string GetPartnerDavcna(Dictionary<string, object> match) { 
            if (match != null && match.ContainsKey("Davčna številka") && !string.IsNullOrEmpty((string)match["Davčna številka"])){
                return (string)match["Davčna številka"];
            }
            return "";
        }

        private async Task PovoziObstojecegaPartnerja(WoocommerceOrder order, string oznaka) {
           
            // you must call UpdateParameters so that the PL gets filled with the correct partner's attributes!
            var pars = await client.sifrant.UpdateParameters(SIFRANTPARTNERJEVPATH, oznaka);

            var pack = await attributeMapper.GetPackedParameters(order);
            pack["txtSifraPartnerja"] = oznaka;
            string sifra = (string)await client.sifrant.Update(SIFRANTPARTNERJEVPATH, pack);
        }

        private async Task<string> UstvariNovegaPartnerja(WoocommerceOrder order, string davcna) {
            var pack = await attributeMapper.GetPackedParameters(order);
            if (!string.IsNullOrEmpty(davcna)) {
                pack["txtDavcnaStevilka"] = davcna;
                pack["IDStevilka"] = davcna;
            }
            string sifra = (string)await client.sifrant.Create(SIFRANTPARTNERJEVPATH, pack);
            return sifra;
        }

        private async Task DodajDavcnoVObstojecegaPartnerja(string davcna, string mtch) {
            var pars = await client.sifrant.UpdateParameters(SIFRANTPARTNERJEVPATH, mtch);
            var packed = pars
            .GroupBy(x => x.Koda)
            .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
            packed["txtDavcnaStevilka"] = davcna;
            await client.sifrant.Update(SIFRANTPARTNERJEVPATH, packed);
        }
    }
}
