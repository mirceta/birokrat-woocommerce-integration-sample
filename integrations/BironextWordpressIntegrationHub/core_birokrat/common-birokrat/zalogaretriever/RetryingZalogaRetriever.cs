using BirokratNext.api_clientv2;
using BirokratNext.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.tools.zalogaretriever {
    public class RetryingZalogaRetriever : IZalogaRetriever {

        IZalogaRetriever zalogaRetriever;

        public RetryingZalogaRetriever(IZalogaRetriever zalogaRetriever) {
            this.zalogaRetriever = zalogaRetriever;
        }
        
        public string Get(string sifra) {
            for (int i = 0; i < 5; i++) {
                try {
                    return zalogaRetriever.Get(sifra);
                } catch (BironextRestartException ex) {
                    if (i == 4)
                        throw ex;
                }
            }
            throw new Exception("Should not have happened ever!");
        }

        public Task<List<Tuple<string, string>>> Query() {
            for (int i = 0; i < 5; i++) {
                try {
                    return zalogaRetriever.Query();
                } catch (BironextRestartException ex) {
                    if (i == 4)
                        throw ex;
                }
            }
            throw new Exception("Should not have happened ever!");
        }

        public Dictionary<string, object> Infer(Dictionary<string, object> state)
        {
            state["zalogaRetriever"] = this;
            return state;
        }


    }
}
