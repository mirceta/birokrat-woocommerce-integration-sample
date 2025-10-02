using Microsoft.EntityFrameworkCore;
using si.birokrat.next.common_database.contexts;

namespace si.birokrat.next.common_database.factories {
    public static class BiroDbContextFactory {
        public static BiroDbContext Create(string connectionString, bool ignoreSyncIdProperty = false, bool ignoreYearCodeProperty = false) {
            if (string.IsNullOrEmpty(connectionString)) {
                return null;
            }

            var optionsBuilder = new DbContextOptionsBuilder<BiroDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new BiroDbContext(optionsBuilder.Options) {
                IgnoreSyncIdProperty = ignoreSyncIdProperty,
                IgnoreYearCodeProperty = ignoreYearCodeProperty
            };
        }
    }
}
