using System.Collections.Generic;

namespace BirokratNext
{
    public class PLParameterResponseRecord {
        public string Opis;
        public string Koda;
        public string Tip;
        public object PrivzetaVrednost;
        public bool HasEvent;
        public bool ReadOnly;
        public List<string> Opcije;
    }
}
