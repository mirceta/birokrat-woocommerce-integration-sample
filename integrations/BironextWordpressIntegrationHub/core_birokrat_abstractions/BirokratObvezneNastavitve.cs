using BirokratNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.common_birokrat
{
    public class BirokratObvezneNastavitve
    {
        Dictionary<string, string> kodaVVrednost;
        PLValueComparator plCompare;
        public BirokratObvezneNastavitve(Dictionary<string, string> kodaVVrednost) {
            this.kodaVVrednost = kodaVVrednost;
            plCompare = new PLValueComparator();
        }

        public async Task Verify(IApiClientV2 client) {
            var tmp = await client.sifrant.Parameters("sifranti/uporabniskenastavitve");

            foreach (var koda in kodaVVrednost.Keys) {
                var matches = tmp.Where(x => x.Koda == koda).ToList();
                if (matches.Count == 0) {
                    throw new Exception(); // katastrofa - ni opcije v uporabniskih nastavitvha
                }
                var match = matches.First();
                string trk = match.PrivzetaVrednost.ToString();
                if (!plCompare.Equals(match, kodaVVrednost[match.Koda])) {
                    throw new Exception($"Nastavitev {match.Opis} mora biti {kodaVVrednost[match.Koda]}, trenutno pa je nastavljena na {(string)match.PrivzetaVrednost}");
                }
            }
        } 
    }

    public class PLValueComparator {
        public PLValueComparator() { 
        
        }

        public bool Equals(PLParameterResponseRecord rec, string val) {
            if (rec.Tip == "boolean") {
                string tmp = rec.PrivzetaVrednost.ToString().ToLower();
                return tmp == val;
            } else {
                return rec.PrivzetaVrednost.ToString() == val;
            }
        }
    }
}
