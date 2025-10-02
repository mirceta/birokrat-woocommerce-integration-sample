using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub;
using BiroWoocommerceHub.structs_wc_to_biro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion.postavke_additions
{
    public class PriceMultiplierByVAT_ForETrgovanje_PostavkaAddOp : IAdditionalOperationOnPostavke {

        ICountryMapper mapper;
        Func<WoocommerceOrder, bool> condition;
        BirokratPostavkaUtils utils;

        public PriceMultiplierByVAT_ForETrgovanje_PostavkaAddOp(BirokratPostavkaUtils utils, ICountryMapper mapper,
            Func<WoocommerceOrder, bool> condition) {
            this.mapper = mapper;
            this.condition = condition;
            this.utils = utils;
        }

        public async Task<List<BirokratPostavka>> ApplyOperationToPostavke(WoocommerceOrder order, List<BirokratPostavka> postavke) {

            if (!this.condition(order))
                return postavke;

            string cntry = order.Data.Billing.Country;

            foreach (var x in postavke) {
                double subtot = utils.GetSubtotal(x);
                subtot *= GetCountryToVAT(cntry);
                x.Subtotal = utils.SerializeToBirokratForm(subtot);
            }
            return postavke;
        }

        private double GetCountryToVAT(string cntry) {
            var some = CountryToVAT.map;
            if (some.ContainsKey(cntry)) {
                return 1 + some[cntry] / 100.0;
            } else {
                return 1.22; // for non eu choose slovenia
            };
        }
    }

    public class PriceMultiplierByVATRatio_PostavkaAddOp : IAdditionalOperationOnPostavke {

        /*
         Uporabi to v naslednjem primeru:
        - stranka noce uporabljati opcije "Pri e-trgovanju privzemi prodajno ceno z davkom"
        - Konkreten problem:
            - v birokrat se vnese cena z davkom, ampak davek je recimo 25% (ker je recimo hrvaska!)
            - Ker je privzeta opcija v birokratu Dobava blaga in storitev, to pomeni, da bo ob vnosu
              Birokrat predpostavljal, da je davek 22%. Kar pomeni recimo, ce smo vnesli 1.22, potem bo
              Birokrat mislil da je bila osnovna cena 1, ceprav je bil v resnici davek 25%, sepravi je
              bila v resnici originalna cena 0.98!
         - Resitev: Uporabi ta razred, da bo koncno ceno delil z 1.22, potem pa mnozil z 1.25.
         - Obrazlozitev: 
            - Primercek kako se izide: Problem je da kupujemo izdelek ki stane 1 evro z davkom vred po hrvaski zakonodaji (25%) davek:
                - Poznamo koncno ceno z davkom Y = 1 evro.
                - birokrat bo predvideval da je osnovna cena X * 1.22 = Y
                - Mi pa iscemo vrednost Z, tako da bo Z * 1.25 = Y
                - Mi moramo Z dobiti tako da je Z * 1.25 = Y
                - ampak ce je X * 1.22 = Y,
                    potem je Z * 1.25 = X * 1.22
                    torej Z = X * 1.22 / 1.25
                 - Torej moramo vhodno ceno z davkom mnoziti s 1.22 / 1.25!, 
                 - KAKO SE BO STVAR IZVEDLA V BIROKARTU? PA POGLEJMO:
                     - V birokrat vnesemo ceno Q = Y * 1.22 / 1.25.
                     - Birokrat predvideva, da je davek 22%, torej misli da je X * 1.22 = Q
                     - Ce vnesemo Q dobimo: X * 1.22 = Y * 1.22 / 1.25
                     - Damo na e-trgovanje in nastavimo drzavo hrvasko!
                     - Birokrat bo odbil 22%, potem pa dodal 25%:
                        X * 1.25 = Y
                     To je tocno to kar smo hoteli => osnovnemu izdelku pribiti 25% namesto 22%!!!
         */

        ICountryMapper mapper;
        Func<WoocommerceOrder, bool> condition;
        BirokratPostavkaUtils utils;

        public PriceMultiplierByVATRatio_PostavkaAddOp(BirokratPostavkaUtils utils, ICountryMapper mapper,
            Func<WoocommerceOrder, bool> condition) {
            this.mapper = mapper;
            this.condition = condition;
            this.utils = utils;
        }

        public async Task<List<BirokratPostavka>> ApplyOperationToPostavke(WoocommerceOrder order, List<BirokratPostavka> postavke) {

            if (!this.condition(order))
                return postavke;

            string cntry = order.Data.Billing.Country;

            foreach (var x in postavke) {
                double subtot = utils.GetSubtotal(x);
                subtot *= (1.22 / GetCountryToVAT(cntry));
                x.Subtotal = utils.SerializeToBirokratForm(subtot);
            }
            return postavke;
        }

        private double GetCountryToVAT(string cntry) {
            var some = CountryToVAT.map;
            if (some.ContainsKey(cntry)) {
                return 1 + some[cntry] / 100.0;
            } else {
                return 1.22; // for non eu choose slovenia
            };
        }
    }

    public static class CountryToVAT
    {
        public static Dictionary<string, double> map = new Dictionary<string, double>() {
                { "AT", 20 },
                { "DE", 19},
                { "BE", 21},
                { "BG", 20},
               {  "CY", 19},
               {  "CZ", 21},
               {  "DK", 25},
              {   "EE", 20},
              {   "GR", 24},
              {   "ES", 21},
              {   "FI", 24},
               {  "FR", 20},
             {    "HR", 25},
              {   "HU", 27},
             {    "IE", 23},
              {   "IT", 22},
             {    "LT", 21},
             {    "LU", 17},
             {    "LV", 21},
             {    "MT", 18},
             {    "NL", 21},
            {     "PL", 23},
             {    "PT", 23},
            {    "RO", 19},
            {     "SE", 25},
             {    "SK", 20}
            };
    }
}
