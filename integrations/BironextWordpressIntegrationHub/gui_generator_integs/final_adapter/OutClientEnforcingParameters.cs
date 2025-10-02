using BiroWoocommerceHubTests;

namespace gui_generator_integs.final_adapter
{
    public class OutClientEnforcingParameters
    {
        public IOutApiClient enforcedClient { get; set; }
        public bool enforceBiroToWoo { get; set; }
        public bool enforceWooToBiro { get; set; }

        public static OutClientEnforcingParameters NoEnforcing()
        {
            return new OutClientEnforcingParameters()
            {
                enforcedClient = null,
                enforceWooToBiro = false,
                enforceBiroToWoo = false
            };
        }
    }




}