using BiroWooHub.logic.integration;
using System.Linq;
using gui_generator;
using System.Threading.Tasks;

namespace gui_generator_integs.final_adapter
{
    public class WithInjectedBironextApiKeys : ILazyIntegrationAdapter
    {

        string apiKey;
        ILazyIntegrationAdapter next;
        public WithInjectedBironextApiKeys(string apiKey, ILazyIntegrationAdapter next)
        {
            this.apiKey = apiKey;
            this.next = next;
        }

        public Task<IIntegration> AdaptFinal(CurrentValue content, string integrationType)
        {
            content.dependencies.Where(x => x.variable == "@@IApiClientV2").Single()
                   .dependencies.Where(x => x.variable == "apiKey").Single()
                   .value = apiKey;
            return next.AdaptFinal(content, integrationType);
        }
    }
}