using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace si.birokrat.next.common_database.models {
    [Table("Partner")]
    public partial class Partner {
        [Column(TypeName = "ntext")]
        public string AlternativnaImena { get; set; }

        [Column("BarkodaeSLOG")]
        public short? BarkodaeSlog { get; set; }

        [Column("BICKoda")]
        [StringLength(20)]
        public string Bickoda { get; set; }

        public double? Cena1 { get; set; }

        public double? Cena2 { get; set; }

        public double? Cena3 { get; set; }

        public double? Cena4 { get; set; }

        [StringLength(25)]
        public string ClanaVpisal { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ClanDatumVpisa { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ClanDo { get; set; }

        [StringLength(20)]
        public string ClanskaStevilka { get; set; }

        [StringLength(50)]
        public string Custom1 { get; set; }

        [StringLength(50)]
        public string Custom2 { get; set; }

        [StringLength(50)]
        public string Custom3 { get; set; }

        [StringLength(50)]
        public string Custom4 { get; set; }

        [StringLength(50)]
        public string Custom5 { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DatumKonca { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DatumNastopa { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DatumRojstva { get; set; }

        [StringLength(12)]
        public string DatumVnosa { get; set; }

        [StringLength(5)]
        public string DavcnaIzpostava { get; set; }

        [StringLength(50)]
        public string DavcnaStevilka { get; set; }

        [Column("DDV")]
        public short? Ddv { get; set; }

        [Column(TypeName = "decimal(12, 6)")]
        public decimal? DelovnaDoba { get; set; }

        [Column("DelovnoDovoljenjeST")]
        [StringLength(30)]
        public string DelovnoDovoljenjeSt { get; set; }

        [StringLength(50)]
        public string DelovnoMesto { get; set; }

        public bool? Detacirani { get; set; }

        [Column(TypeName = "ntext")]
        public string DirektorijPriloge { get; set; }

        public double? Dodatek1 { get; set; }

        public double? Dodatek2 { get; set; }

        public double? Dodatek3 { get; set; }

        public double? Dodatek4 { get; set; }

        [StringLength(50)]
        public string Dodatno1 { get; set; }

        [StringLength(50)]
        public string Dodatno2 { get; set; }

        [Column(TypeName = "decimal(12, 6)")]
        public decimal? Dopust { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DovoljenjeIzdanoDne { get; set; }

        [Column("DovoljenjeVeljaDO", TypeName = "datetime")]
        public DateTime? DovoljenjeVeljaDo { get; set; }

        [Column("DovoljenjeVeljaOD", TypeName = "datetime")]
        public DateTime? DovoljenjeVeljaOd { get; set; }

        public short? DrugiDelodajalec { get; set; }

        [StringLength(3)]
        public string Drzava { get; set; }

        [StringLength(3)]
        public string DrzavaDetasirani { get; set; }

        [StringLength(3)]
        public string DrzavaRezidenstva { get; set; }

        [StringLength(30)]
        public string Drzavljanstvo { get; set; }

        public short? DvigniCenoZaRabat { get; set; }

        [StringLength(150)]
        public string Email { get; set; }

        [Column("EMSO")]
        [StringLength(32)]
        public string Emso { get; set; }

        [Column("eSLOG")]
        public short? ESlog { get; set; }

        [StringLength(60)]
        public string Fax { get; set; }

        [Column("GLNKoda")]
        [StringLength(50)]
        public string Glnkoda { get; set; }

        [Column("H_ST")]
        [StringLength(5)]
        public string HSt { get; set; }

        [Column(TypeName = "ntext")]
        public string HitraOpomba { get; set; }

        public short? Hotelir { get; set; }

        [Column("IDStevilka")]
        [StringLength(22)]
        public string Idstevilka { get; set; }

        [Column("IME")]
        [StringLength(50)]
        public string Ime { get; set; }

        public bool? InternetDa { get; set; }

        public bool? Invalid { get; set; }

        public short? InvalidNadKvoto { get; set; }

        [Column(TypeName = "decimal(12, 6)")]
        public decimal? IzkorisceniDopust { get; set; }

        public short? JeKontakt { get; set; }

        [StringLength(50)]
        public string Komentar1 { get; set; }

        [StringLength(50)]
        public string Komentar2 { get; set; }

        [Column(TypeName = "ntext")]
        public string KomentarPopusta { get; set; }

        [Column(TypeName = "ntext")]
        public string KomentarZnizanjaTakse { get; set; }

        [StringLength(4)]
        public string Komercialist { get; set; }

        [Column(TypeName = "decimal(12, 6)")]
        public decimal? KonkurencnaKlavzula { get; set; }

        public short? Konsignatar { get; set; }

        [StringLength(60)]
        public string Kontakt { get; set; }

        [StringLength(50)]
        public string Kraj { get; set; }

        [StringLength(50)]
        public string KrajRojstva { get; set; }

        [StringLength(50)]
        public string KrajZacasnegaBivalisca { get; set; }

        [StringLength(100)]
        public string LetnaNarocilnica { get; set; }

        public short? MamicaDo3Leta { get; set; }

        [StringLength(20)]
        public string MaticnaStevilka { get; set; }

        [StringLength(32)]
        public string Mesto { get; set; }

        public short? NacinProdaje { get; set; }

        [StringLength(20)]
        public string Naziv { get; set; }

        public bool? Nerezident { get; set; }

        public short? NeUporabljaj { get; set; }

        [Column("NeUpostevajZaIOP")]
        public short? NeUpostevajZaIop { get; set; }

        [Column("NoceUPenzijo")]
        public short? NoceUpenzijo { get; set; }

        [StringLength(5)]
        public string ObcinaBivanja { get; set; }

        [StringLength(1)]
        public string Obrazec { get; set; }

        [Column("OdjavljenIzZZZS", TypeName = "datetime")]
        public DateTime? OdjavljenIzZzzs { get; set; }

        [StringLength(80)]
        public string OdprtPri { get; set; }

        public double? OmejitevNeplacano { get; set; }

        public double? OmejitevZapadlo { get; set; }

        [Column("OmogociPlaciloZDobavnico")]
        public short? OmogociPlaciloZdobavnico { get; set; }

        [Column(TypeName = "ntext")]
        public string Opombe { get; set; }

        [Column(TypeName = "ntext")]
        public string OpozoriloZaRacun { get; set; }

        [StringLength(25)]
        public string OpozoriUporabnik { get; set; }

        public short? OpozoriZapadlo { get; set; }

        [StringLength(50)]
        public string OsebnaIzkaznica { get; set; }

        public double? Otroci { get; set; }

        [Column("OZNAKA")]
        [StringLength(10)]
        public string Oznaka { get; set; }

        [Column("Partner")]
        [StringLength(52)]
        public string Partner1 { get; set; }

        [Column("Partner1")]
        [StringLength(80)]
        public string Partner11 { get; set; }

        [Column("PE")]
        [StringLength(25)]
        public string Pe { get; set; }

        public short? PlacilniRok { get; set; }

        public short? PlacilniRokNaOdpremo { get; set; }

        [StringLength(50)]
        public string Pogodba { get; set; }

        [Column("PogodbaOPoslovodenju")]
        public short? PogodbaOposlovodenju { get; set; }

        [StringLength(30)]
        public string Poklic { get; set; }

        public short? Popust { get; set; }

        [StringLength(10)]
        public string Posta { get; set; }

        [StringLength(10)]
        public string PostaZacasnegaBivalisca { get; set; }

        [StringLength(50)]
        public string PotniList { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? PotniListDatum { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? PotniListDo { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? PotniListOd { get; set; }

        public double? PovecanaDobaProcent { get; set; }

        public bool? PovecanaSplosnaOlajsava { get; set; }

        [StringLength(1)]
        public string PravniStatus { get; set; }

        [Column("PrenesenoIzPOSa")]
        public byte? PrenesenoIzPosa { get; set; }

        [Column(TypeName = "decimal(12, 6)")]
        public decimal? PreostaliDopust { get; set; }

        [Column("PRIIMEK")]
        [StringLength(50)]
        public string Priimek { get; set; }

        [Column("PrijavljenNaZZZS", TypeName = "datetime")]
        public DateTime? PrijavljenNaZzzs { get; set; }

        public double? RabatGeneralno { get; set; }

        public int? RabatnaSkupina { get; set; }

        [Key]
        public int RecNo { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? Rojen { get; set; }

        [Column("RojenVKraju")]
        [StringLength(25)]
        public string RojenVkraju { get; set; }

        [StringLength(10)]
        public string Sifra { get; set; }

        [StringLength(5)]
        public string SifraDrzave { get; set; }

        [StringLength(5)]
        public string SifraDrzaveNaslova { get; set; }

        [StringLength(5)]
        public string SifraPojavnegaStatusa { get; set; }

        [Column("SKIS")]
        [StringLength(6)]
        public string Skis { get; set; }

        [StringLength(32)]
        public string Sklic { get; set; }

        [StringLength(25)]
        public string Skupina { get; set; }

        public short? Spol { get; set; }

        public bool? Sprememba { get; set; }

        [Column(TypeName = "decimal(12, 6)")]
        public decimal? Stalnost { get; set; }

        [Column(TypeName = "decimal(12, 6)")]
        public decimal? StalnostTrenutno { get; set; }

        [StringLength(30)]
        public string StevilkaOsebnegaDokumenta { get; set; }

        public short? StevilkaRacuna { get; set; }

        public double? Stimulac { get; set; }

        [StringLength(40)]
        public string StopnjaIzobrazbe { get; set; }

        [Column(TypeName = "decimal(12, 6)")]
        public decimal? SuperRabat { get; set; }

        [StringLength(60)]
        public string Telefon { get; set; }

        [StringLength(50)]
        public string Telefon2 { get; set; }

        public double? TempCena1 { get; set; }

        public double? TempCena2 { get; set; }

        public double? TempCena3 { get; set; }

        public short? TezavnostDela { get; set; }
        [Column("TKDIS", TypeName = "ntext")]

        public string Tkdis { get; set; }
        [StringLength(70)]

        public string Ulica { get; set; }
        [Column(TypeName = "datetime")]

        public DateTime? VarnostPriDelu { get; set; }

        [Column("VarnostPriDeluST")]
        [StringLength(30)]
        public string VarnostPriDeluSt { get; set; }

        [Column("VarnostPriDeluVeljaDO", TypeName = "datetime")]
        public DateTime? VarnostPriDeluVeljaDo { get; set; }

        [Column("VarnostPriDeluVeljaOD", TypeName = "datetime")]
        public DateTime? VarnostPriDeluVeljaOd { get; set; }

        [StringLength(50)]
        public string Viza { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? VizaDatum { get; set; }

        [Column("VizaDO", TypeName = "datetime")]
        public DateTime? VizaDo { get; set; }

        [Column("VizaOD", TypeName = "datetime")]
        public DateTime? VizaOd { get; set; }

        [Column("VIzvrsbi")]
        public short? Vizvrsbi { get; set; }

        [Column("VIzvrsbiOd", TypeName = "datetime")]
        public DateTime? VizvrsbiOd { get; set; }

        [StringLength(50)]
        public string Vnasalec { get; set; }

        [Column("VnesenoNaPOSu")]
        public byte? VnesenoNaPosu { get; set; }

        [StringLength(32)]
        public string Vrsta { get; set; }

        [StringLength(2)]
        public string VrstaHonorarja { get; set; }

        public short? VrstaIzplacila { get; set; }

        [StringLength(20)]
        public string VrstaNaslova { get; set; }

        public short? VrstaOsebe { get; set; }

        [StringLength(20)]
        public string VrstaOsebnegaDokumenta { get; set; }

        [Column("VrstaPoslaZBS")]
        [StringLength(2)]
        public string VrstaPoslaZbs { get; set; }

        [StringLength(50)]
        public string VrstaUre { get; set; }

        [StringLength(50)]
        public string VrstaZaposlitve { get; set; }

        public short? VzdrzevaniOdrasli { get; set; }

        [StringLength(32)]
        public string ZacasnoBivalisce { get; set; }

        public short? Zaposlen { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ZdravstveniPregled { get; set; }

        [Column("ZdravstveniPregledST")]
        [StringLength(30)]
        public string ZdravstveniPregledSt { get; set; }

        [Column("ZdravstveniPregledVeljaDO", TypeName = "datetime")]
        public DateTime? ZdravstveniPregledVeljaDo { get; set; }

        [Column("ZdravstveniPregledVeljaOD", TypeName = "datetime")]
        public DateTime? ZdravstveniPregledVeljaOd { get; set; }

        [Column("Ziro_Racun")]
        [StringLength(60)]
        public string ZiroRacun { get; set; }

        [Column("Ziro_Racun1")]
        [StringLength(60)]
        public string ZiroRacun1 { get; set; }

        [Column("Ziro_Racun2")]
        [StringLength(60)]
        public string ZiroRacun2 { get; set; }

        public short? ZnizanjeTakse { get; set; }

        [StringLength(50)]
        public string AlterTelefon { get; set; }

        public short? MladiDo30 { get; set; }

        [Column("PogodbaOPoslovodenju2014_18Clen")]
        public short? PogodbaOposlovodenju201418clen { get; set; }

        [Column("SP1LetoOprostitev30")]
        public short? Sp1letoOprostitev30 { get; set; }

        [Column("SP1LetoOprostitev50")]
        public short? Sp1letoOprostitev50 { get; set; }

        [Column("SP1OdMinimalne2014")]
        public short? Sp1odMinimalne2014 { get; set; }

        public Guid SyncId { get; set; }

        [Required]
        [StringLength(5)]
        public string YearCode { get; set; }
    }
}
