using si.birokrat.next.common_database.models.biromaster;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace si.birokrat.next.common_database.models {
    [Table("entity_company")]
    public partial class EntityCompany {
        public EntityCompany() {
            AccountCompany = new HashSet<MapCompanyAccount>();
        }

        [Key]
        [Column("pk_entity_company_id")]
        public int Id { get; set; }

        [Column("created_dt", TypeName = "datetime")]
        public DateTime CreatedDt { get; set; }

        [Column("modified_dt", TypeName = "datetime")]
        public DateTime ModifiedDt { get; set; }

        [Required]
        [Column("name")]
        [StringLength(250)]
        public string Name { get; set; }

        [Required]
        [Column("tax_number")]
        [StringLength(25)]
        public string TaxNumber { get; set; }

        [Column("version")]
        public int Version { get; set; }

        [Column("version_local")]
        public int? VersionLocal { get; set; }

        [Required]
        [Column("is_active")]
        public bool? IsActive { get; set; }

        [StringLength(50)]
        public string Password { get; set; }
        public bool? Hotelir { get; set; }

        [StringLength(50)]
        public string AdminPassword { get; set; }
        
        public List<EntityCompanyYear> EntityCompanyYear { get; set; }

        [Required]
        [Column("sync_ts")]
        public byte[] SyncTs { get; set; }

        [InverseProperty("Company")]
        public ICollection<MapCompanyAccount> AccountCompany { get; set; }
    }
}
