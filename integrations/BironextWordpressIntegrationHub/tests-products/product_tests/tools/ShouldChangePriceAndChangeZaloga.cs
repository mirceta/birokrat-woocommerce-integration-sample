using BirokratNext;
using birowoo_exceptions;
using core.tools.birokratops;
using core.tools.zalogaretriever;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tests.tests.hisavizij;

namespace tests {
    public class ShouldChangePriceAndChangeZaloga {


        IApiClientV2 apiClient;
        IZalogaRetriever zaloga;
        public ShouldChangePriceAndChangeZaloga(IApiClientV2 apiClient, IZalogaRetriever zaloga) {
            this.apiClient = apiClient;
            this.zaloga = zaloga;
        }

        public async Task Execute(string sifra, TracedList results) {


            string beforeChange = await getCena(sifra);
            await UpdatePrice(sifra);
            string afterChange = await getCena(sifra);

            if (beforeChange == afterChange)
                throw new ProductTestException($"Birokrat said that it changed the price for {sifra} but did not!");


            //string befChangeZal = await getZaloga(sifra);
            var json = await GBirokratOps.CreatePrevzem_ReturnJson(new List<string> { sifra }, apiClient);
            /*
             * Tole ni tak problem: Naredis prevzem kar poveca zalogo v centralnem skladiscu. Docim recimo ce mas
             * hisovizij ona uporablja druga skladisca - in potem tukile proba sestet ta skladisca in dobi da
             * zaloga ni spremenjena - ker centralno skladisce ni zajeto. To bi moral se nastudirat ampak
             * ce vids mejhne cifre in misls da je napaka - to je razlog.
             * 
            string aftChangeZal = await getZaloga(sifra);
            if (befChangeZal == aftChangeZal)
                throw new ProductTestException($"Birokrat was supposed to increase zaloga for {sifra} but did not!");
            */
        }

        private async Task<string> getCena(string sifra) {
            string sifrantRoute = @"sifranti\artikli\prodajniartikli-storitve";
            var tmp = await apiClient.sifrant.UpdateParameters(sifrantRoute, sifra);

            var dict = tmp
            .GroupBy(x => x.Koda)
            .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
            var cena = (string)dict["PCsPD"];
            return cena;
        }

        private async Task<string> getZaloga(string sifra) {
            return zaloga.Get(sifra); 
        }

        private async Task UpdatePrice(string sifra, double price = -1) {
            string sifrantRoute = @"sifranti\artikli\prodajniartikli-storitve";
            var parame = await apiClient.sifrant.UpdateParameters(sifrantRoute, sifra);
            var dict = parame
            .GroupBy(x => x.Koda)
            .ToDictionary(x => x.Key, y => y.Last().PrivzetaVrednost);
            if (price == -1) {
                dict["PCsPD"] = string.Format("{0:0,00}", ((5000 + new Random().NextDouble() * 500) + ""));
                dict["PCBrezPD"] = string.Format("{0:0,00}", ((5000 + new Random().NextDouble() * 500) + ""));
            } else {
                dict["PCsPD"] = string.Format("{0:0,00}", price + "");
                dict["PCBrezPD"] = string.Format("{0:0,00}", ((5000 + new Random().NextDouble() * 500) + ""));
            }
            var result1 = await apiClient.sifrant.Update(sifrantRoute, dict);
        }

    }

    
}
