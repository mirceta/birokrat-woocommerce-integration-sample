using BironextWordpressIntegrationHub.structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic.eslog_gen
{
    public class Preamble
    {
        public static string Get(string woo_date, string additionalNumber, string woo_city, string UID) {

            string date = woo_date;
            date = date.Substring(0, date.IndexOf(".")).Replace(' ', 'T');
            return $@"
                    <PoljubnoBesedilo>
                        <VrstaBesedila>AAI</VrstaBesedila>
                        <Besedilo>
                            <Tekst1>PRIROCNIK_ESLOG_1_6_1</Tekst1>
                        </Besedilo>
                    </PoljubnoBesedilo>
                    <GlavaRacuna>
                        <VrstaRacuna>380</VrstaRacuna>
                        <StevilkaRacuna>{$"{additionalNumber}"}</StevilkaRacuna>
                        <eSlogGUID>{UID}</eSlogGUID>
                        <FunkcijaRacuna>9</FunkcijaRacuna>
                        <NacinPlacila>0</NacinPlacila>
                        <KodaNamena>OTHR</KodaNamena>
                    </GlavaRacuna>
                    <DatumiRacuna>
                        <VrstaDatuma>137</VrstaDatuma>
                        <DatumRacuna>{date}</DatumRacuna>
                    </DatumiRacuna>
                    <DatumiRacuna>
                        <VrstaDatuma>35</VrstaDatuma>
                        <DatumRacuna>{date}</DatumRacuna>
                    </DatumiRacuna>
                    <DatumiRacuna>
                        <VrstaDatuma>325</VrstaDatuma>
                        <DatumRacuna>{date}</DatumRacuna>
                    </DatumiRacuna>
                    <Valuta>
                        <VrstaValuteRacuna>2</VrstaValuteRacuna>
                        <KodaValute>EUR</KodaValute>
                    </Valuta>
                    <Lokacije>
                        <VrstaLokacije>91</VrstaLokacije>
                        <NazivLokacije>{woo_city}</NazivLokacije>
                    </Lokacije>
                    <PlacilniPogoji>
                        <PodatkiORokih>
                            <VrstaPogoja>3</VrstaPogoja>
                        </PodatkiORokih>
                        <PlacilniRoki>
                            <VrstaDatumaPlacilnegaRoka>13</VrstaDatumaPlacilnegaRoka>
                            <Datum>{date}</Datum>
                        </PlacilniRoki>
                    </PlacilniPogoji>
                    <PoljubnoBesedilo>
                        <VrstaBesedila>AAI</VrstaBesedila>
                        <Besedilo>
                            <Tekst1></Tekst1>
                            <Tekst2></Tekst2>
                        </Besedilo>
                    </PoljubnoBesedilo>
                    <PoljubnoBesedilo>
                        <VrstaBesedila>AAI</VrstaBesedila>
                        <Besedilo>
                            <Tekst1></Tekst1>
                            <Tekst2></Tekst2>
                            <Tekst3></Tekst3>
                            <Tekst4></Tekst4>
                            <Tekst5></Tekst5>
                        </Besedilo>
                    </PoljubnoBesedilo>
                    <PoljubnoBesedilo>
                        <VrstaBesedila>AAI</VrstaBesedila>
                        <Besedilo>
                            <Tekst1></Tekst1>
                            <Tekst2></Tekst2>
                            <Tekst3></Tekst3>
                            <Tekst4></Tekst4>
                            <Tekst5></Tekst5>
                        </Besedilo>
                    </PoljubnoBesedilo>
                    <PoljubnoBesedilo>
                        <VrstaBesedila>ZZZ</VrstaBesedila>
                        <Besedilo>
                            <Tekst1></Tekst1>
                            <Tekst2></Tekst2>
                            <Tekst3></Tekst3>
                            <Tekst4></Tekst4>
                            <Tekst5></Tekst5>
                        </Besedilo>
                    </PoljubnoBesedilo>
            ";
        }
    }
}
