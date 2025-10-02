using System;
using System.Threading.Tasks;

namespace tests.tools.fixture_setup.synchronized {
    public class SynchronizedDatabaseResetter {

        DatabaseOps ops;
        public SynchronizedDatabaseResetter(DatabaseOps ops) {
            this.ops = ops;
        }

        public async Task<bool> Reset(string settingsFilePath, string localsql, string localbackuppath) {
            lock (ops) {
               ops.ResetDatabase(settingsFilePath, localsql, localbackuppath);
            }
            return true;
        }
    }
}
