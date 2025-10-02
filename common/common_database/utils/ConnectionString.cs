using Microsoft.Extensions.Configuration;

namespace si.birokrat.next.common_database.utils {
    public static class ConnectionString {
        public static string Format(IConfiguration configuration, string dbContext, string database = "") {
            string server = configuration[$"{dbContext}:Server"];
            if (string.IsNullOrEmpty(database)) {
                database = configuration[$"{dbContext}:Database"];
            }
            string username = configuration[$"{dbContext}:Username"];
            string password = configuration[$"{dbContext}:Password"];

            return common.database.ConnectionString.Format(server, database, username, password);
        }
    }
}
