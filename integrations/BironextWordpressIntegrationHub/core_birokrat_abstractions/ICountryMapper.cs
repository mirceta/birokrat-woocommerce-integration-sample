using gui_inferable;
using System.Threading.Tasks;

namespace BiroWoocommerceHub
{
    public interface ICountryMapper : IInferable {
        Task<string> Map(string value);
    }
}
