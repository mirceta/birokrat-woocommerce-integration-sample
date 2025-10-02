using BirokratNext;
using BironextWordpressIntegrationHub.structs;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic
{
    public class PartnerInserterHelper {

        const string SIFRANTPARTNERJEVPATH = @"sifranti\poslovnipartnerjiinosebe\poslovnipartnerji";

        IApiClientV2 client;
        IPartnerWooToBiroMapper mapper;
        
        public PartnerInserterHelper(IApiClientV2 client,
            IPartnerWooToBiroMapper mapper) {
            this.client = client;
            this.mapper = mapper;
        }

        public async Task<string> UstvariNovegaPartnerja(WoocommerceOrder order, string davcna) {
            var pack = await mapper.GetPackedParameters(order);
            if (!string.IsNullOrEmpty(davcna)) {
                pack["txtDavcnaStevilka"] = davcna;
                pack["IDStevilka"] = davcna;
            }
            string sifra = (string)await client.sifrant.Create(SIFRANTPARTNERJEVPATH, pack);
            return sifra;
        }

        public async Task PovoziObstojecegaPartnerja(WoocommerceOrder order, string oznaka) {

            // you must call UpdateParameters so that the PL gets filled with the correct partner's attributes!
            var pars = await client.sifrant.UpdateParameters(SIFRANTPARTNERJEVPATH, oznaka);

            var pack = await mapper.GetPackedParameters(order);
            pack["txtSifraPartnerja"] = oznaka;
            string sifra = (string)await client.sifrant.Update(SIFRANTPARTNERJEVPATH, pack);
        }
    }
}
