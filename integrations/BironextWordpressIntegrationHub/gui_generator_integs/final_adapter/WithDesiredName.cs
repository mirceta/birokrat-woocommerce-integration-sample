using BiroWooHub.logic.integration;
using System.Linq;
using gui_generator;
using System.Threading.Tasks;

namespace gui_generator_integs.final_adapter
{
    internal class WithDesiredName : ILazyIntegrationAdapter
    {

        string desiredName;
        ILazyIntegrationAdapter next;
        public WithDesiredName(string desiredName, ILazyIntegrationAdapter next)
        {
            this.desiredName = desiredName;
            this.next = next;
        }

        public Task<IIntegration> AdaptFinal(CurrentValue content, string integrationType)
        {
            content.dependencies.Where(x => x.variable == "entry").Single()
                    .dependencies.Where(x => x.variable == "name").Single()
                    .value = desiredName;
            return next.AdaptFinal(content, integrationType);
        }
    }
}