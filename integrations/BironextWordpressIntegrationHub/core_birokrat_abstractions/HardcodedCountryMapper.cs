using birowoo_exceptions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiroWoocommerceHub
{
    public class HardcodedCountryMapper : ICountryMapper
    {

        string defaultCountry;
        public HardcodedCountryMapper(string defaultCountry = null) {
            this.defaultCountry = defaultCountry;
        }
        public Dictionary<string, object> Infer(Dictionary<string, object> state)
        {
            state["countryMapper"] = this;
            return state;
        }
        public async Task<string> Map(string value) {
            if (value == "HR")
                return "HRV";
            else if (value == "SI")
                return "SLO";

            if (defaultCountry != null)
                return defaultCountry;
            throw new IntegrationProcessingException("This country is not handled");
        }
    }
}
