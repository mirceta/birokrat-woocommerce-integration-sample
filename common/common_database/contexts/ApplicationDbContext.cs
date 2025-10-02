using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using si.birokrat.next.common_database.models;

namespace si.birokrat.next.common_database.contexts {
    public class ApplicationDbContext : IdentityDbContext<Company, ApplicationRole, int> {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<Company>().ToTable("Companies");
        }
    }
}
