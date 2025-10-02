using si.birokrat.next.common.build;
using System.IO;
using System.Threading.Tasks;
using tests.tools.fixture_setup.synchronized;

namespace tests.tests.estrada
{
    public class DatabaseResetStage : ISetupStage {
        string settingsFilePath;
        string localsql;
        string localbackuppath;
        SynchronizedDatabaseResetter resetter;
        public DatabaseResetStage(SynchronizedDatabaseResetter resetter, 
            string settingsFilePath,
            string localsql,
            string localbackuppath) {
            
            this.settingsFilePath = Path.Combine(Build.SolutionPath, "tests_fixture", "database_configs", settingsFilePath);
            this.localsql = localsql;
            this.localbackuppath = localbackuppath;
            this.resetter = resetter;

        }
        
        public async Task Work() {
            await resetter.Reset(settingsFilePath, localsql, localbackuppath);
        }
    }
}
