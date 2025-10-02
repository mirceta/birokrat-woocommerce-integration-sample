using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tests.tools.fixture_setup.synchronized {
    public class SynchronizedBironextResetter {

        IBironextDriver driver;
        string localSqlServer;

        DateTime lastRestarted;

        bool locked = false;
        
        public SynchronizedBironextResetter(IBironextDriver driver, string localSqlServer) {
            this.driver = driver;
            this.localSqlServer = localSqlServer;
            this.lastRestarted = DateTime.MinValue;
        }

        public async Task<bool> Reset() {
            
            if (locked)
                return false;
            if (DateTime.Now - lastRestarted < TimeSpan.FromMinutes(1))
                return true;

            Console.WriteLine("Entered bironext resetter");
            locked = true;
            try {
                await execute();
                lastRestarted = DateTime.Now;
                Console.WriteLine("Bironext reset successful!");
            } catch (Exception ex) {
                locked = false;
                Console.WriteLine("Bironext reset failure!");
                throw ex;
            }
            locked = false;
            return true;
        }


        async Task execute() {
            driver.Kill();
            Thread.Sleep(10000);
            driver.Start(localSqlServer);
            Thread.Sleep(10000);
            await driver.Validate(true);
        }
        
    }
}
