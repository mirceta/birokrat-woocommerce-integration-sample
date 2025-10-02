using System;
using System.Collections.Generic;
using System.Text;

namespace gui_generator_integs
{
    internal class StaticVariables
    {
        public static List<string> desiredExtractedVariableTypes = new List<string> {
                "IOutApiClient",
                "IApiClientV2",
                "ICountryMapper",
                "IVatIdParser",
                "IZalogaRetriever"
            };
    }
}
