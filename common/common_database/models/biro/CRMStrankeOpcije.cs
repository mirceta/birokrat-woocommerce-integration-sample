using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace si.birokrat.next.common_database.models {
    [Table("CRMStrankeOpcije")]
    public partial class CRMStrankeOpcije {
        public short? Aktivno { get; set; }

        [StringLength(50)]
        public string Aplikacija { get; set; }

        [StringLength(12)]
        public string DatumVnosa { get; set; }

        public short? Level { get; set; }

        [StringLength(10)]
        public string Opcija { get; set; }

        [StringLength(50)]
        public string OpisPolja { get; set; }

        [Key]
        public int Recno { get; set; }

        [StringLength(10)]
        public string Sifra { get; set; }

        [StringLength(50)]
        public string Vnasalec { get; set; }

        [StringLength(100)]
        public string Vrednost { get; set; }

        public short? Zaporedje { get; set; }

        public Guid SyncId { get; set; }

        [Required]
        [StringLength(5)]
        public string YearCode { get; set; }
    }
}
