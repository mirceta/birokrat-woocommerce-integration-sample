using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace si.birokrat.next.common_database.models {
    [Table("security_account")]
    public partial class SecurityAccount {
        public SecurityAccount() {
            AccountCompany = new HashSet<MapCompanyAccount>();
        }

        [Key]
        [Column("pk_security_account_id")]
        public int Id { get; set; }

        [Column("created_dt", TypeName = "datetime")]
        public DateTime CreatedDt { get; set; }

        [Column("modified_dt", TypeName = "datetime")]
        public DateTime ModifiedDt { get; set; }

        [Required]
        [Column("user_name")]
        [StringLength(250)]
        public string UserName { get; set; }

        [Required]
        [Column("password")]
        [StringLength(128)]
        public string Password { get; set; }

        [Column("is_admin")]
        public bool IsAdmin { get; set; }

        [Required]
        [Column("is_enabled")]
        public bool? IsEnabled { get; set; }

        [InverseProperty("Account")]
        public ICollection<MapCompanyAccount> AccountCompany { get; set; }
    }
}
