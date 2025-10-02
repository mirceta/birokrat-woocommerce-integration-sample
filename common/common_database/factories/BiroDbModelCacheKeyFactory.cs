using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using si.birokrat.next.common_database.contexts;

namespace si.birokrat.next.common_database.factories {
    public class BiroDbModelCacheKeyFactory : IModelCacheKeyFactory {
        public object Create(DbContext context) {
            if (context is BiroDbContext biroDbContext) {
                return (context.GetType(), biroDbContext.IgnoreSyncIdProperty, biroDbContext.IgnoreYearCodeProperty);
            }
            return context.GetType();
        }
    }
}
