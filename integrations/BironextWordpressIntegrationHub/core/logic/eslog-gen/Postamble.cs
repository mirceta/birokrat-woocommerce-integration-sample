using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic.eslog_gen
{
    public class Postamble
    {
        public static string Get() {
            return $@"
        <PovzetekDavkovRacuna>
            <DavkiRacuna>
                <VrstaDavka>VAT</VrstaDavka>
                <OdstotekDavka>22.00</OdstotekDavka>
            </DavkiRacuna>
            <ZneskiDavkov>
                <VrstaZneskaDavka>124</VrstaZneskaDavka>
                <ZnesekDavka>3.57</ZnesekDavka>
            </ZneskiDavkov>
            <ZneskiDavkov>
                <VrstaZneskaDavka>125</VrstaZneskaDavka>
                <ZnesekDavka>16.21</ZnesekDavka>
            </ZneskiDavkov>
        </PovzetekDavkovRacuna>
        <PovzetekZneskovRacuna>
            <ZneskiRacuna>
                <VrstaZneska>9</VrstaZneska>
                <ZnesekRacuna>19.77</ZnesekRacuna>
            </ZneskiRacuna>
            <SklicZaPlacilo>
                <SklicPlacila>PQ</SklicPlacila>
                <StevilkaSklica>SI0000005-00033-00001</StevilkaSklica>
            </SklicZaPlacilo>
        </PovzetekZneskovRacuna>
        <PovzetekZneskovRacuna>
            <ZneskiRacuna>
                <VrstaZneska>86</VrstaZneska>
                <ZnesekRacuna>19.77</ZnesekRacuna>
            </ZneskiRacuna>
            <SklicZaPlacilo>
                <SklicPlacila>PQ</SklicPlacila>
            </SklicZaPlacilo>
        </PovzetekZneskovRacuna>
        <PovzetekZneskovRacuna>
            <ZneskiRacuna>
                <VrstaZneska>125</VrstaZneska>
                <ZnesekRacuna>16.20</ZnesekRacuna>
            </ZneskiRacuna>
            <SklicZaPlacilo>
                <SklicPlacila>PQ</SklicPlacila>
            </SklicZaPlacilo>
        </PovzetekZneskovRacuna>
        <PovzetekZneskovRacuna>
            <ZneskiRacuna>
                <VrstaZneska>176</VrstaZneska>
                <ZnesekRacuna>3.56</ZnesekRacuna>
            </ZneskiRacuna>
            <SklicZaPlacilo>
                <SklicPlacila>PQ</SklicPlacila>
            </SklicZaPlacilo>
        </PovzetekZneskovRacuna>
        <PovzetekZneskovRacuna>
            <ZneskiRacuna>
                <VrstaZneska>79</VrstaZneska>
                <ZnesekRacuna>16.20</ZnesekRacuna>
            </ZneskiRacuna>
            <SklicZaPlacilo>
                <SklicPlacila>PQ</SklicPlacila>
            </SklicZaPlacilo>
        </PovzetekZneskovRacuna>
        <PovzetekZneskovRacuna>
            <ZneskiRacuna>
                <VrstaZneska>53</VrstaZneska>
                <ZnesekRacuna>0.00</ZnesekRacuna>
            </ZneskiRacuna>
            <SklicZaPlacilo>
                <SklicPlacila>PQ</SklicPlacila>
            </SklicZaPlacilo>
        </PovzetekZneskovRacuna>
            ";
        }
    }
}
