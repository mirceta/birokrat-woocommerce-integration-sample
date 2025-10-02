using BiroWooHub.logic.integration;
using System.Linq;
using gui_generator;
using System.Threading.Tasks;

namespace gui_generator_integs.final_adapter
{
    internal class WithBironextAddress : ILazyIntegrationAdapter
    {
        string bironextAddress;
        ILazyIntegrationAdapter next;
        public WithBironextAddress(string bironextAddress, ILazyIntegrationAdapter next)
        {
            this.bironextAddress = bironextAddress;
            this.next = next;
        }
        public Task<IIntegration> AdaptFinal(CurrentValue content, string integrationType)
        {
            content.dependencies.Where(x => x.variable == "@@IApiClientV2").Single()
                   .dependencies.Where(x => x.variable == "apiAddress").Single()
                   .value = bironextAddress;
            return next.AdaptFinal(content, integrationType);
        }
    }
}