using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace si.birokrat.next.common_database.models {
    [Table("SifreOperaterjev")]
    public partial class SifreOperaterjev {
        [Column(TypeName = "ntext")]
        public string Bliznjice { get; set; }

        public short? BrezNabavnihCen { get; set; }

        public short? CalcOnly { get; set; }

        [StringLength(10)]
        public string DatumVnosa { get; set; }

        [Column(TypeName = "ntext")]
        public string Dostop { get; set; }

        [Column(TypeName = "ntext")]
        public string DostopVpis { get; set; }

        public int? DovoljenjeZaOdobritevNaloga { get; set; }

        public short? DovoljenoSpreminjanjeStrank { get; set; }

        [StringLength(20)]
        public string Geslo { get; set; }

        [StringLength(10)]
        public string HitraPrijava { get; set; }

        [StringLength(25)]
        public string HitraPrijava2 { get; set; }

        [StringLength(50)]
        public string ImeZaposlenega { get; set; }

        public short? ItemIndex { get; set; }

        public short? Neaktiven { get; set; }

        [StringLength(50)]
        public string No { get; set; }

        public short? No1 { get; set; }

        [StringLength(50)]
        public string Oddelek { get; set; }

        public short? OmogociAnalize { get; set; }

        public short? OmogociToolbar { get; set; }

        public short? OnemogociExcel { get; set; }

        public short? OnemogociIzpisSifrantov { get; set; }

        public short? OnemogociIzpisSifrantovArtiklov { get; set; }

        public short? OnemogociKadre { get; set; }

        [Column("OnemogociPE")]
        public short? OnemogociPe { get; set; }

        public int? OnemogociPonovitevIzpisaRacuna { get; set; }

        public short? OnemogociStorno { get; set; }

        public short? OnemogociStornoPostavk { get; set; }

        public short? OnemogociVnosPartnerjev { get; set; }

        public short? OnemogociVnosProdajnihCen { get; set; }

        public short? OnemogociVnosSifrantov { get; set; }

        public short? OnemogociVnosVrst { get; set; }

        public short? OnemogociVnosVse { get; set; }

        [StringLength(50)]
        public string Operater { get; set; }

        [StringLength(50)]
        public string Opis { get; set; }

        [StringLength(3)]
        public string Oznaka { get; set; }

        [StringLength(5)]
        public string OznakaLeta { get; set; }

        [StringLength(5)]
        public string OznakaLetaIzvor { get; set; }

        [Column("PCName")]
        [StringLength(50)]
        public string Pcname { get; set; }

        public short? PrikaziOpozorilaNeplacano { get; set; }

        [Key]
        public int RecNo { get; set; }

        public int? SamoCas { get; set; }

        public short? SamoTekoce { get; set; }

        [StringLength(50)]
        public string SkupinaArtiklov { get; set; }

        public short? SpremembaPodatkov { get; set; }

        [StringLength(50)]
        public string Temp { get; set; }

        public short? TihiZakljucek { get; set; }

        public int? ToDo { get; set; }

        [Column(TypeName = "ntext")]
        public string Toolbar { get; set; }

        public int? VeljavenCenik { get; set; }

        [StringLength(50)]
        public string VeljavnaPredloga { get; set; }

        [StringLength(50)]
        public string Vnasalec { get; set; }

        [StringLength(50)]
        public string VrstaPartnerjev { get; set; }

        public Guid SyncId { get; set; }

        [Column("FURSDavcnaStevilka")]
        [StringLength(10)]
        public string FursdavcnaStevilka { get; set; }

        [Column("FURSJeProdajalecTujec")]
        public short? FursjeProdajalecTujec { get; set; }

        [Required]
        [StringLength(5)]
        public string YearCode { get; set; }
    }
}
