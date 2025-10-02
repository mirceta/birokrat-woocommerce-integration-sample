using BiroWooHub.logic.integration;
using tests.tests.estrada;

namespace tests.tools
{
    public interface IProgressKeeperFactory {
        IProgressKeeper Create(IIntegration integration);
    }
}
