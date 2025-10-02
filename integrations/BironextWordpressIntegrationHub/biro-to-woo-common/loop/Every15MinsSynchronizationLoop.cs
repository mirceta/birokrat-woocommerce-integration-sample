using BiroWooHub.logic.integration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo.loop {
    public class Every15MinsSynchronizationLoop {

        ISynchronization sychronization;

        public Every15MinsSynchronizationLoop(
            ISynchronization synchronization) {
            this.sychronization = synchronization;
        }

        public async Task Execute() {
            while (true) {
                try {
                    DateTime start = DateTime.Now;
                    await sychronization.Work();
                    int passed = (int)DateTime.Now.Subtract(start).TotalSeconds;
                    int tosleep = (15 * 60) - passed;
                    if (tosleep > 0) {
                        Log.Information($"Sleeping for {tosleep} seconds.");
                        Thread.Sleep(tosleep * 1000);
                    }
                } catch (Exception ex) {
                    Log.Error("Failure during main loop (outmost loop log entry)" + ex.Message + ex.StackTrace);
                    Thread.Sleep(30000);
                }
            }
        }

        

    }
}
