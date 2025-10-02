using System;
using System.Collections.Generic;
using System.Text;

namespace core.logic.mapping_biro_to_woo
{
    public interface IBirokratProductChangeHandler
    {
        void HandleChange(Dictionary<string, object> biroArtikel, Dictionary<string, object> obj, Dictionary<string, object> wooload);
    }
}
