using System.Collections.Generic;
using System.Threading.Tasks;
using infrastructure_pinger.deployment;

namespace infrastructure_pinger
{
    public abstract class Deployment : IPingable {

        public bool AwaitingResolution = false;
        public int UnsuccessfulPingsInARow = 0;
        public string PingResult;

        public string Name;
        public Dictionary<string, string> AdditionalInfo;

        public Deployment(string Name, Dictionary<string, string> AdditionalInfo) {
            this.Name = Name;
            this.AdditionalInfo = AdditionalInfo;
        }

        public virtual Task<string> Ping()
        {
            throw new System.NotImplementedException();
        }
    }
}
