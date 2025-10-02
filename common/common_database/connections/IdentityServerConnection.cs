using Microsoft.Extensions.Configuration;
using si.birokrat.next.common.database;

namespace si.birokrat.next.common_database.connections {
    public class IdentityServerConnection : Connection {
        public IdentityServerConnection(IConfiguration configuration)
            : base(utils.ConnectionString.Format(configuration, "ApplicationDb")) { }
    }
}
