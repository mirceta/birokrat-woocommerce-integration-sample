using gui_inferable;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace core.tools.zalogaretriever
{
    public interface IZalogaRetriever : IInferable
    {
        string Get(string sifra);
        Task<List<Tuple<string, string>>> Query();
    }
}