using System.Collections.Generic;

namespace tests.tests.estrada
{
    public interface IProgressKeeper {
        void Setup();
        bool IsAlreadyProcessed(string signature);
        void SaveState(ProgressState state);
        List<ProgressState> GetFullState();
        void Restart();
    }
}
