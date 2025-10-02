using System;
using System.Collections.Generic;
using System.Text;

namespace si.birokrat.next.common.biro {
    public class BiroVersion {

        int major;
        int minor;
        int build;
        int revision;

        public BiroVersion(string version) {

            if (version.Contains("|")) {
                version = version.Split('|')[1];
            }

            string[] parts = version.Split('.');
            major = int.Parse(parts[0]);
            minor = int.Parse(parts[1]);
            build = int.Parse(parts[2]);
            
        }

        public bool IsGreaterOrEquals(String version) {
            BiroVersion contender = new BiroVersion(version);

            if (major > contender.major)
                return true;
            if (major < contender.major)
                return false;

            if (minor > contender.minor)
                return true;
            if (minor < contender.minor)
                return false;

            if (build > contender.build)
                return true;
            if (build < contender.build)
                return false;            

            return true;
        }

    }
}
