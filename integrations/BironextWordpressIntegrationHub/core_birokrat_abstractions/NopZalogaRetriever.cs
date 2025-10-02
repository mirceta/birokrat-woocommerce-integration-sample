using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.tools.zalogaretriever
{
    public class NopZalogaRetriever : IZalogaRetriever
    {
        public  string Get(string sifra)
        {
            throw new NotImplementedException();
        }

        public  Task<List<Tuple<string, string>>> Query()
        {
            throw new NotImplementedException();
        }
        public Dictionary<string, object> Infer(Dictionary<string, object> state)
        {
            state["zalogaRetriever"] = this;
            return state;
        }
    }
}
