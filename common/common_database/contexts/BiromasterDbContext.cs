using Microsoft.EntityFrameworkCore;
using si.birokrat.next.common_database.models;
using si.birokrat.next.common_database.models.biromaster;

namespace si.birokrat.next.common_database.contexts {
    public partial class BiromasterDbContext : DbContext {
        public virtual DbSet<EntityCompany> Company { get; set; }
        public virtual DbSet<EntityCompanyYear> EntityCompanyYear { get; set; }
        public virtual DbSet<MapCompanyAccount> AccountCompany { get; set; }
        public virtual DbSet<SecurityAccount> Account { get; set; }
        public virtual DbSet<SecurityApplication> Application { get; set; }

        public BiromasterDbContext(DbContextOptions<BiromasterDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<EntityCompany>(entity => {
                entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.ModifiedDt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SyncTs).IsRowVersion();
            });

            modelBuilder.Entity<EntityCompanyYear>(entity => {
                entity.HasKey(e => e.PkEntityCompanyYearId);

                entity.ToTable("entity_company_year");

                entity.Property(e => e.PkEntityCompanyYearId).HasColumnName("pk_entity_company_year_id");

                entity.Property(e => e.CreatedDt)
                    .HasColumnName("created_dt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FkEntityCompanyId).HasColumnName("fk_entity_company_id");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("is_active")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LocalVersion).HasColumnName("local_version");

                entity.Property(e => e.ModifiedDt)
                    .HasColumnName("modified_dt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RemotePartnershipId).HasColumnName("remote_partnership_id");

                entity.Property(e => e.RemoteVersion).HasColumnName("remote_version");

                entity.Property(e => e.SyncTs)
                    .IsRequired()
                    .HasColumnName("sync_ts")
                    .IsRowVersion();

                entity.Property(e => e.Year).HasColumnName("year");

                entity.Property(e => e.YearCode)
                    .IsRequired()
                    .HasColumnName("year_code")
                    .HasMaxLength(10);

                entity.HasOne(d => d.FkEntityCompany)
                    .WithMany(p => p.EntityCompanyYear)
                    .HasForeignKey(d => d.FkEntityCompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_entity_company_year_entity_company");
            });

            modelBuilder.Entity<MapCompanyAccount>(entity => {
                entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifiedDt).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.AccountCompany)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_map_company_account_entity_company");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountCompany)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_map_company_account_security_account");
            });

            modelBuilder.Entity<SecurityAccount>(entity => {
                entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsEnabled).HasDefaultValueSql("((1))");

                entity.Property(e => e.ModifiedDt).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<SecurityApplication>(entity => {
                entity.Property(e => e.CreatedDt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsEnabled).HasDefaultValueSql("((1))");

                entity.Property(e => e.ModifiedDt).HasDefaultValueSql("(getdate())");
            });
        }
    }
}
