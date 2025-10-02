using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace si.birokrat.next.common_database.models {
    [Table("security_application")]
    public partial class SecurityApplication {
        [Key]
        [Column("pk_security_application_id")]
        public int Id { get; set; }

        [Column("created_dt", TypeName = "datetime")]
        public DateTime CreatedDt { get; set; }

        [Column("modified_dt", TypeName = "datetime")]
        public DateTime ModifiedDt { get; set; }

        [Required]
        [Column("code")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        [Column("name")]
        [StringLength(250)]
        public string Name { get; set; }

        [Required]
        [Column("is_enabled")]
        public bool? IsEnabled { get; set; }
    }
}
