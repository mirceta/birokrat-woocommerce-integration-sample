using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace si.birokrat.next.common_database.models {
    [Table("map_company_account")]
    public partial class MapCompanyAccount {
        [Key]
        [Column("pk_map_company_account_id")]
        public long Id { get; set; }

        [Column("fk_entity_company_id")]
        public int CompanyId { get; set; }

        [Column("fk_security_account_id")]
        public int AccountId { get; set; }

        [Column("created_dt", TypeName = "datetime")]
        public DateTime CreatedDt { get; set; }

        [Column("modified_dt", TypeName = "datetime")]
        public DateTime ModifiedDt { get; set; }

        [Column("is_admin")]
        public bool IsAdmin { get; set; }

        [ForeignKey("CompanyId")]
        [InverseProperty("AccountCompany")]
        public EntityCompany Company { get; set; }

        [ForeignKey("AccountId")]
        [InverseProperty("AccountCompany")]
        public SecurityAccount Account { get; set; }
    }
}
