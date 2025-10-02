using BironextWordpressIntegrationHub.structs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic
{
    public interface IPartnerWooToBiroMapper {
        Task<Dictionary<string, object>> GetPackedParameters(WoocommerceOrder order);
        Task<string> GetNaziv(WoocommerceOrder order);
        Task<string> GetUlica(WoocommerceOrder order);
        Task<string> GetDodatekNaziva(WoocommerceOrder order);
    }
}
