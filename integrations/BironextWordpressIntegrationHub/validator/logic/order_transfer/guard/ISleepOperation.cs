using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace validator.logic.order_transfer.guard {
    public interface ISleepOperation {
        Task Sleep();
    }

    public class ThreadSleepOperation : ISleepOperation {

        int millis;
        public ThreadSleepOperation(int millis) {
            this.millis = millis;
        }
        public async Task Sleep() {
            Thread.Sleep(millis);
        }
    }
}
