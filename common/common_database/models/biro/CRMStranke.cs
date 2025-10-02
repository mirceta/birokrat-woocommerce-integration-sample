using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace si.birokrat.next.common_database.models {
    [Table("CRMStranke")]
    public partial class CRMStranke {
        [Column(TypeName = "datetime")]
        public DateTime? DatumIztekaPogodbe { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DatumLicence { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DatumPavsala { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DatumVeljavnostiLicence { get; set; }

        [StringLength(50)]
        public string DatumVnosa { get; set; }

        [StringLength(49)]
        public string ImeLicence { get; set; }

        [StringLength(50)]
        public string InternetGeslo { get; set; }

        [StringLength(60)]
        public string InternetUsername { get; set; }

        [StringLength(50)]
        public string Logo { get; set; }

        public short? Maloprodaja { get; set; }

        public short? MreznaVerzija { get; set; }

        public short? NeObjavi { get; set; }

        public short? NeUporablja { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? Obisk1 { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? Obisk2 { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? Obisk3 { get; set; }

        [Column(TypeName = "ntext")]
        public string OpisObisk1 { get; set; }

        [Column(TypeName = "ntext")]
        public string OpisObisk2 { get; set; }

        [Column(TypeName = "ntext")]
        public string OpisObisk3 { get; set; }

        [Column(TypeName = "ntext")]
        public string Opombe { get; set; }

        [StringLength(5)]
        public string OznakaLetaIzvora { get; set; }

        [StringLength(60)]
        public string Partner { get; set; }

        [StringLength(100)]
        public string Polje1 { get; set; }

        [StringLength(100)]
        public string Polje10 { get; set; }

        [StringLength(100)]
        public string Polje2 { get; set; }

        [StringLength(100)]
        public string Polje3 { get; set; }

        [StringLength(100)]
        public string Polje4 { get; set; }

        [StringLength(100)]
        public string Polje5 { get; set; }

        [StringLength(100)]
        public string Polje6 { get; set; }

        [StringLength(100)]
        public string Polje7 { get; set; }

        [StringLength(100)]
        public string Polje8 { get; set; }

        [StringLength(100)]
        public string Polje9 { get; set; }

        [StringLength(50)]
        public string Prodajalec { get; set; }

        public short? Proizvodnja { get; set; }

        public short? Racunovodstvo { get; set; }

        [Key]
        public int RecNo { get; set; }

        [StringLength(10)]
        public string Sifra { get; set; }

        [StringLength(6)]
        public string SifraPosrednika { get; set; }

        [StringLength(50)]
        public string StevilkaPogodbe { get; set; }

        [StringLength(50)]
        public string Vnasalec { get; set; }

        public Guid SyncId { get; set; }

        [Required]
        [StringLength(5)]
        public string YearCode { get; set; }
    }
}
