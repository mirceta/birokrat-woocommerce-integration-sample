using Serilog;
using si.birokrat.next.common.logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo.loop {
    public class EveryXSecondsLoop : MyLoggable {

        ISynchronization sychronization;
        int seconds;
        public EveryXSecondsLoop(
            int seconds,
            ISynchronization synchronization) {
            this.sychronization = synchronization;
            this.seconds = seconds;
        }

        public async Task Execute() {
            while (true) {
                try {
                    DateTime start = DateTime.Now;
                    await sychronization.Work();
                    int passed = (int)DateTime.Now.Subtract(start).TotalSeconds;
                    int tosleep = (seconds) - passed;
                    if (tosleep > 0) {
                        Log.Information($"Sleeping for {tosleep} seconds.");
                        Thread.Sleep(tosleep * 1000);
                    }
                } catch (Exception ex) {
                    Console.WriteLine("Failure during main loop (outmost loop log entry)" + ex.Message + ex.StackTrace);
                    Log.Error("Failure during main loop (outmost loop log entry)" + ex.Message + ex.StackTrace);
                    Thread.Sleep(30000);
                }
            }
        }



    }
}
