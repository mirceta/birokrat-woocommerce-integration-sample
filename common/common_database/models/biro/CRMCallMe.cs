using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace si.birokrat.next.common_database.models {
    [Table("CRMCallMe")]
    public partial class CRMCallMe {
        [Column(TypeName = "datetime")]

        public DateTime? DatumNarocila { get; set; }

        [StringLength(50)]
        public string ImeIzvrsil { get; set; }

        [StringLength(50)]
        public string ImeUporabnika { get; set; }

        public short? IzlociIzSeznama { get; set; }

        [StringLength(5)]
        public string Izvrsil { get; set; }

        [StringLength(50)]
        public string Oddelek { get; set; }

        [StringLength(5)]
        public string OznakaIzvrsil { get; set; }

        [StringLength(5)]
        public string OznakaUporabnika { get; set; }

        [Column(TypeName = "ntext")]
        public string Razlog { get; set; }

        [Key]
        public int Recno { get; set; }

        public short? SteviloKlicev { get; set; }

        [StringLength(100)]
        public string Stranka { get; set; }

        [StringLength(50)]
        public string Telefon { get; set; }

        [StringLength(50)]
        public string TelefonKlicani { get; set; }

        [Column("TelefonKlicaniID")]
        [StringLength(50)]
        public string TelefonKlicaniId { get; set; }

        [StringLength(5)]
        public string Uporabnik { get; set; }

        public Guid SyncId { get; set; }

        [Required]
        [StringLength(5)]
        public string YearCode { get; set; }
    }
}
