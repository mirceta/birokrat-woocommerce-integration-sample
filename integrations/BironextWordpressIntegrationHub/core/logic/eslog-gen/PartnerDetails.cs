using BironextWordpressIntegrationHub.structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic.eslog_gen
{
    public class PartnerDetails
    {
        public static string Preamble() {
            // empty because Birokrat will fill it by itself
            return $@"
                <PodatkiPodjetja>
                    <NazivNaslovPodjetja>
                        <VrstaPartnerja>II</VrstaPartnerja>
                        <NazivPartnerja>
                            <NazivPartnerja1></NazivPartnerja1>
                            <NazivPartnerja2></NazivPartnerja2>
                            <NazivPartnerja3 />
                            <NazivPartnerja4 />
                        </NazivPartnerja>
                        <Ulica>
                            <Ulica1></Ulica1>
                            <Ulica2></Ulica2>
                            <Ulica3 />
                            <Ulica4 />
                        </Ulica>
                        <Kraj></Kraj>
                        <NazivDrzave></NazivDrzave>
                        <PostnaStevilka></PostnaStevilka>
                        <KodaDrzave></KodaDrzave>
                    </NazivNaslovPodjetja>
                    <FinancniPodatkiPodjetja>
                        <BancniRacun>
                            <StevilkaBancnegaRacuna></StevilkaBancnegaRacuna>
                            <NazivBanke1></NazivBanke1>
                            <NazivBanke2></NazivBanke2>
                            <BIC>BAKOSI2XXXX</BIC>
                        </BancniRacun>
                    </FinancniPodatkiPodjetja>
                    <ReferencniPodatkiPodjetja>
                        <VrstaPodatkaPodjetja>VA</VrstaPodatkaPodjetja>
                        <PodatekPodjetja></PodatekPodjetja>
                    </ReferencniPodatkiPodjetja>
                    <ReferencniPodatkiPodjetja>
                        <VrstaPodatkaPodjetja>GN</VrstaPodatkaPodjetja>
                        <PodatekPodjetja></PodatekPodjetja>
                    </ReferencniPodatkiPodjetja>
                    <KontaktiPodjetja>
                        <KontaktnaOseba>
                            <ImeOsebe></ImeOsebe>
                        </KontaktnaOseba>
                        <Komunikacije>
                            <StevilkaKomunikacije></StevilkaKomunikacije>
                            <VrstaKomunikacije>TE</VrstaKomunikacije>
                        </Komunikacije>
                        <Komunikacije>
                            <StevilkaKomunikacije></StevilkaKomunikacije>
                            <VrstaKomunikacije>EM</VrstaKomunikacije>
                        </Komunikacije>
                    </KontaktiPodjetja>
                </PodatkiPodjetja>
            ";
        }

        public static string Billing(Billing billing) {

            return $@"
                <PodatkiPodjetja>
                    <NazivNaslovPodjetja>
                        <VrstaPartnerja>BY</VrstaPartnerja>
                        <NazivPartnerja>
                            <NazivPartnerja1>{billing.FirstName} {billing.LastName}</NazivPartnerja1>
                            <NazivPartnerja2></NazivPartnerja2>
                            <NazivPartnerja3 />
                            <NazivPartnerja4 />
                        </NazivPartnerja>
                        <Ulica>
                            <Ulica1>{billing.Address1}</Ulica1>
                            <Ulica2></Ulica2>
                            <Ulica3 />
                            <Ulica4 />
                        </Ulica>
                        <Kraj>{billing.City}</Kraj>
                        <NazivDrzave>{CountryCodeToCountry(billing.Country)}</NazivDrzave>
                        <PostnaStevilka>{billing.Postcode}</PostnaStevilka>
                        <KodaDrzave>{billing.Country}</KodaDrzave>
                    </NazivNaslovPodjetja>
                    <FinancniPodatkiPodjetja>
                        <BancniRacun>
                            <StevilkaBancnegaRacuna></StevilkaBancnegaRacuna>
                            <NazivBanke1></NazivBanke1>
                            <NazivBanke2></NazivBanke2>
                            <BIC>BAKOSI2XXXX</BIC>
                        </BancniRacun>
                    </FinancniPodatkiPodjetja>
                    <ReferencniPodatkiPodjetja>
                        <VrstaPodatkaPodjetja>VA</VrstaPodatkaPodjetja>
                        <PodatekPodjetja></PodatekPodjetja>
                    </ReferencniPodatkiPodjetja>
                    <ReferencniPodatkiPodjetja>
                        <VrstaPodatkaPodjetja>GN</VrstaPodatkaPodjetja>
                        <PodatekPodjetja></PodatekPodjetja>
                    </ReferencniPodatkiPodjetja>
                    <KontaktiPodjetja>
                        <KontaktnaOseba>
                            <ImeOsebe></ImeOsebe>
                        </KontaktnaOseba>
                        <Komunikacije>
                            <StevilkaKomunikacije></StevilkaKomunikacije>
                            <VrstaKomunikacije>TE</VrstaKomunikacije>
                        </Komunikacije>
                        <Komunikacije>
                            <StevilkaKomunikacije>{billing.Email}</StevilkaKomunikacije>
                            <VrstaKomunikacije>EM</VrstaKomunikacije>
                        </Komunikacije>
                    </KontaktiPodjetja>
                </PodatkiPodjetja>
            ";
        }

        public static string Shipping(Shipping shipping) {

            // argument is woocommerce data
            return $@"
                <PodatkiPodjetja>
                    <NazivNaslovPodjetja>
                        <VrstaPartnerja>IV</VrstaPartnerja>
                        <NazivPartnerja>
                            <NazivPartnerja1>{shipping.FirstName} {shipping.LastName}</NazivPartnerja1>
                            <NazivPartnerja2></NazivPartnerja2>
                            <NazivPartnerja3 />
                            <NazivPartnerja4 />
                        </NazivPartnerja>
                        <Ulica>
                            <Ulica1>{shipping.Address1}</Ulica1>
                            <Ulica2></Ulica2>
                            <Ulica3 />
                            <Ulica4 />
                        </Ulica>
                        <Kraj>{shipping.City}</Kraj>
                        <NazivDrzave>{CountryCodeToCountry(shipping.Country)}</NazivDrzave>
                        <PostnaStevilka>{shipping.Postcode}</PostnaStevilka>
                        <KodaDrzave>{shipping.Country}</KodaDrzave>
                    </NazivNaslovPodjetja>
                    <FinancniPodatkiPodjetja>
                        <BancniRacun>
                            <StevilkaBancnegaRacuna></StevilkaBancnegaRacuna>
                            <NazivBanke1></NazivBanke1>
                            <NazivBanke2></NazivBanke2>
                            <BIC>BAKOSI2XXXX</BIC>
                        </BancniRacun>
                    </FinancniPodatkiPodjetja>
                    <ReferencniPodatkiPodjetja>
                        <VrstaPodatkaPodjetja>VA</VrstaPodatkaPodjetja>
                        <PodatekPodjetja></PodatekPodjetja>
                    </ReferencniPodatkiPodjetja>
                    <ReferencniPodatkiPodjetja>
                        <VrstaPodatkaPodjetja>GN</VrstaPodatkaPodjetja>
                        <PodatekPodjetja></PodatekPodjetja>
                    </ReferencniPodatkiPodjetja>
                    <KontaktiPodjetja>
                        <KontaktnaOseba>
                            <ImeOsebe></ImeOsebe>
                        </KontaktnaOseba>
                        <Komunikacije>
                            <StevilkaKomunikacije></StevilkaKomunikacije>
                            <VrstaKomunikacije>TE</VrstaKomunikacije>
                        </Komunikacije>
                        <Komunikacije>
                            <StevilkaKomunikacije></StevilkaKomunikacije>
                            <VrstaKomunikacije>EM</VrstaKomunikacije>
                        </Komunikacije>
                    </KontaktiPodjetja>
                </PodatkiPodjetja>
            ";
        }

        private static string CountryCodeToCountry(string country) {
            return GCountryMapper.Map(country);
        }
    }
}
