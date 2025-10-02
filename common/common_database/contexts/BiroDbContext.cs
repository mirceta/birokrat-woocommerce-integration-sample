using Microsoft.EntityFrameworkCore;
using si.birokrat.next.common_database.models;
using si.birokrat.next.common_database.models.biro;

namespace si.birokrat.next.common_database.contexts {
    public partial class BiroDbContext : DbContext {
        public BiroDbContext(DbContextOptions options)
            : base(options) { }

        public bool? IgnoreSyncIdProperty { get; set; }

        public bool? IgnoreYearCodeProperty { get; set; }

        public virtual DbSet<CRMCallMe> CRMCallMe { get; set; }

        public virtual DbSet<CRMStranke> CRMStranke { get; set; }

        public virtual DbSet<CRMStrankeOpcije> CRMStrankeOpcije { get; set; }

        public virtual DbSet<Partner> Partner { get; set; }

        public virtual DbSet<SifreOperaterjev> SifreOperaterjev { get; set; }

        public virtual DbSet<Slike> Slike { get; set; }

        public virtual DbSet<PostnaKnjiga> PostnaKnjiga { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<CRMCallMe>(entity => {
                if (IgnoreSyncIdProperty.HasValue && IgnoreSyncIdProperty.Value) {
                    entity.Ignore(e => e.SyncId);
                } else {
                    entity.HasIndex(e => e.SyncId)
                        .HasName("ix_CRMCallMe_sync")
                        .IsUnique();

                    entity.Property(e => e.SyncId).HasDefaultValueSql("(newid())");
                }

                if (IgnoreYearCodeProperty.HasValue && IgnoreYearCodeProperty.Value) {
                    entity.Ignore(e => e.YearCode);
                }
            });

            modelBuilder.Entity<CRMStranke>(entity => {
                if (IgnoreSyncIdProperty.HasValue && IgnoreSyncIdProperty.Value) {
                    entity.Ignore(e => e.SyncId);
                } else {
                    entity.HasIndex(e => e.SyncId)
                        .HasName("ix_CRMStranke_sync")
                        .IsUnique();

                    entity.Property(e => e.SyncId).HasDefaultValueSql("(newid())");
                }

                if (IgnoreYearCodeProperty.HasValue && IgnoreYearCodeProperty.Value) {
                    entity.Ignore(e => e.YearCode);
                }
            });

            modelBuilder.Entity<CRMStrankeOpcije>(entity => {
                if (IgnoreSyncIdProperty.HasValue && IgnoreSyncIdProperty.Value) {
                    entity.Ignore(e => e.SyncId);
                } else {
                    entity.HasIndex(e => e.SyncId)
                        .HasName("ix_CRMStrankeOpcije_sync")
                        .IsUnique();

                    entity.Property(e => e.SyncId).HasDefaultValueSql("(newid())");
                }

                if (IgnoreYearCodeProperty.HasValue && IgnoreYearCodeProperty.Value) {
                    entity.Ignore(e => e.YearCode);
                }
            });

            modelBuilder.Entity<Partner>(entity => {
                if (IgnoreSyncIdProperty.HasValue && IgnoreSyncIdProperty.Value) {
                    entity.Ignore(e => e.SyncId);
                } else {
                    entity.HasIndex(e => e.SyncId)
                        .HasName("ix_Partner_sync")
                        .IsUnique();

                    entity.Property(e => e.SyncId).HasDefaultValueSql("(newid())");
                }

                if (IgnoreYearCodeProperty.HasValue && IgnoreYearCodeProperty.Value) {
                    entity.Ignore(e => e.YearCode);
                }
            });

            modelBuilder.Entity<SifreOperaterjev>(entity => {
                if (IgnoreSyncIdProperty.HasValue && IgnoreSyncIdProperty.Value) {
                    entity.Ignore(e => e.SyncId);
                } else {
                    entity.HasIndex(e => e.SyncId)
                        .HasName("ix_SifreOperaterjev_sync")
                        .IsUnique();

                    entity.Property(e => e.SyncId).HasDefaultValueSql("(newid())");
                }
                
                if (IgnoreYearCodeProperty.HasValue && IgnoreYearCodeProperty.Value) {
                    entity.Ignore(e => e.YearCode);
                }
            });
        }
    }
}
