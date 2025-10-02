using System;
using System.Threading.Tasks;
using System.Threading;
using tests.tools.fixture_setup;
using tests.tools.fixture_setup.synchronized;
using lib_pinger;
using infrastructure_pinger;
using tests_bironext_pinger;

namespace tests.tests.estrada {
    public class TestEnvironmentParams {
        string localsql;
        string localbackuppath;
        IBironextDriver driver;
        bool resetdb;
        bool resetnext;
        bool resetprogress;
        DatabaseOps ops;
        SynchronizedDatabaseResetter databaseResetter;
        public TestEnvironmentParams(string localsql, string localbackuppath, IBironextDriver driver, 
            bool resetdb, bool resetnext, bool resetprogress, DatabaseOps ops,
            SynchronizedDatabaseResetter databaseResetter) {
            if (string.IsNullOrEmpty(localsql))
                throw new ArgumentNullException("localsql");
            if (string.IsNullOrEmpty(localbackuppath))
                throw new ArgumentNullException("localbackuppath");
            if (driver == null) {
                throw new ArgumentNullException("driver");
            }
            if (ops == null) {
                throw new ArgumentNullException("ops");
            }
            if (databaseResetter == null) {
                throw new ArgumentNullException("databaseResetter");
            }
            this.localsql = localsql;
            this.localbackuppath = localbackuppath;
            this.driver = driver;
            this.resetdb = resetdb;
            this.resetnext = resetnext;
            this.resetprogress = resetprogress;
            this.ops = ops;
            this.DatabaseResetter = databaseResetter;
        }
        
        public async Task RunSetup_Then_ValidateSetupSuccessful(string stranka, string bironextAddress) {
            
            ops.Validate(stranka.Split("_")[0].ToLower(), Localsql, Localbackuppath);
            Console.WriteLine("Database validation successful");
            if (!await driver.Validate()) {
                Console.WriteLine("Bironext is down... attempting restart");
                driver.Kill();
                driver.Start(Localsql);
                await Task.Delay(10000);
                await driver.Validate(true);
                Console.WriteLine("Bironext is up");
            }
            Console.WriteLine("Bironext is up");
        }

        public string Localsql { get => localsql;  }
        public string Localbackuppath { get => localbackuppath; }
        public bool Resetdb { get => resetdb; }
        public bool Resetnext { get => resetnext; }
        public bool Resetprogress { get => resetprogress; }
        public SynchronizedDatabaseResetter DatabaseResetter { get => databaseResetter; set => databaseResetter = value; }
        public LocalMode_BironextPinger Pinger { get; set; }
    }
}
