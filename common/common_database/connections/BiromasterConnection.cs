using Microsoft.Extensions.Configuration;
using si.birokrat.next.common.database;

namespace si.birokrat.next.common_database.connections {
    public class BiromasterConnection : Connection {
        public BiromasterConnection(IConfiguration configuration)
            : base(utils.ConnectionString.Format(configuration, "BiromasterDb")) { }
    }
}
