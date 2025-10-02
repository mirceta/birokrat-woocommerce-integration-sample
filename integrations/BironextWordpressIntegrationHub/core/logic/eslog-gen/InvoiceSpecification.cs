using BiroWoocommerceHub.flows;
using BiroWoocommerceHub.structs_wc_to_biro;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic.eslog_gen
{
    public class InvoiceSpecification
    {

        public static string Get(IList<BirokratPostavka> postavke) {
            var x = postavke.Select((x, i) => postavkeRacunaTemplate(x, i));
            return string.Join('\n', x.ToArray());
        }

        public static string GetWithPricesAndDiscounts(IList<BirokratPostavka> postavke) {
            var x = postavke.Select((x, i) => postavkeRacunaTemplatePricesAndDiscountsIncluded(x, i));
            return string.Join('\n', x.ToArray());
        }

        private static string postavkeRacunaTemplatePricesAndDiscountsIncluded(BirokratPostavka x, int stevilkaVrstice) {

            string vrednost1 = (Tools.ParseDoubleBigBrainTime(x.Subtotal) * (1 - 0.01 * x.DiscountPercent)).ToString();
            string odstotek1 = x.DiscountPercent.ToString();

            // so original is x * 0.8 = vrednost1
            // vrednost1 / odstotek1 = orignalprice

            return postavkaTemplate(x, stevilkaVrstice, vrednost1, odstotek1);
        }

        private static string postavkeRacunaTemplate(BirokratPostavka x, int stevilkaVrstice) {
            return postavkaTemplate(x, stevilkaVrstice, "", "");
        }

        private static string postavkaTemplate(BirokratPostavka x, int stevilkaVrstice, string vrednost1, string odstotek1) {
            string vrednost2 = "";
            //string vrednost3 = x.Total; // ending price
            string vrednost4 = "";
            string vrednost5 = "";
            string vrednost6 = "";
            string odstotek2 = "";

            return $@"
                <PostavkeRacuna>
                    <Postavka>
                        <StevilkaVrstice>{stevilkaVrstice}</StevilkaVrstice>
                    </Postavka>
                    <DodatnaIdentifikacijaArtikla>
                        <VrstaPodatkaArtikla>5</VrstaPodatkaArtikla>
                        <StevilkaArtiklaDodatna>{x.BirokratSifra}</StevilkaArtiklaDodatna>
                        <VrstaKodeArtiklaDodatna>SA</VrstaKodeArtiklaDodatna>
                    </DodatnaIdentifikacijaArtikla>
                    <OpisiArtiklov>
                        <KodaOpisaArtikla>F</KodaOpisaArtikla>
                        <OpisArtikla>
                            <VrstaArtikla>CU</VrstaArtikla>
                            <OpisArtikla1></OpisArtikla1>
                            <OpisArtikla2></OpisArtikla2>
                        </OpisArtikla>
                    </OpisiArtiklov>
                    <KolicinaArtikla>
                        <VrstaKolicine>47</VrstaKolicine>
                        <Kolicina>{x.Quantity}</Kolicina>
                        <EnotaMere>PCE</EnotaMere>
                    </KolicinaArtikla>
                    <ZneskiPostavke>
                        <VrstaZneskaPostavke>38</VrstaZneskaPostavke>
                        <ZnesekPostavke>{vrednost1}</ZnesekPostavke>
                    </ZneskiPostavke>
                    <ZneskiPostavke>
                        <VrstaZneskaPostavke>203</VrstaZneskaPostavke>
                        <ZnesekPostavke>{vrednost2}</ZnesekPostavke>
                    </ZneskiPostavke>
                    <CenaPostavke>
                        <VrstaCene>AAA</VrstaCene>
                        <Cena></Cena>
                    </CenaPostavke>
                    <CenaPostavke>
                        <VrstaCene>AAB</VrstaCene>
                        <Cena>{vrednost4}</Cena>
                    </CenaPostavke>
                    <DavkiPostavke>
                        <DavkiNaPostavki>
                            <VrstaDavkaPostavke>VAT</VrstaDavkaPostavke>
                            <OdstotekDavkaPostavke>22.00</OdstotekDavkaPostavke>
                        </DavkiNaPostavki>
                        <ZneskiDavkovPostavke>
                            <VrstaZneskaDavkaPostavke>125</VrstaZneskaDavkaPostavke>
                            <Znesek>{vrednost5}</Znesek>
                        </ZneskiDavkovPostavke>
                        <ZneskiDavkovPostavke>
                            <VrstaZneskaDavkaPostavke>124</VrstaZneskaDavkaPostavke>
                            <Znesek>{vrednost6}</Znesek>
                        </ZneskiDavkovPostavke>
                    </DavkiPostavke>
                    <OdstotkiPostavk>
                        <Identifikator>A</Identifikator>
                        <VrstaOdstotkaPostavke>1</VrstaOdstotkaPostavke>
                        <OdstotekPostavke>{odstotek1}</OdstotekPostavke>
                        <VrstaZneskaOdstotka>204</VrstaZneskaOdstotka>
                        <ZnesekOdstotka>{odstotek2}</ZnesekOdstotka>
                    </OdstotkiPostavk>
                </PostavkeRacuna>
            ";
        }
    }
}
