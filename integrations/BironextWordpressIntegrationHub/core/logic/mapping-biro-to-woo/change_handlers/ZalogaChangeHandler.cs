using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Text;

namespace core.logic.mapping_biro_to_woo.change_handlers
{
    public class ZalogaChangeHandler : IBirokratProductChangeHandler
    {
        public void HandleChange(Dictionary<string, object> biroArtikel, Dictionary<string, object> obj, Dictionary<string, object> wooload)
        {
            string birozaloga = (string)biroArtikel["zaloga"];
            if (string.IsNullOrEmpty(birozaloga)) birozaloga = "0";
            string woozaloga = GWooOps.SerializeIntWooProperty(obj["stock_quantity"]);

            Console.WriteLine($"Zaloga detector: biro={birozaloga} outregprice={woozaloga}");
            if (woozaloga != birozaloga)
            {
                wooload["manage_stock"] = "true";
                wooload["stock_quantity"] = birozaloga;
                Console.WriteLine($"Zaloga detector: change detected");
            }
        }
    }
}
