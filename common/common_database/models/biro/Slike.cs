using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace si.birokrat.next.common_database.models.biro {
    public class Slike {
        [Key]
        public int RecNo { get; set; }
        public string Vsebina { get; set; }
        public string Oznaka { get; set; }
        public string Vrsta { get; set; }
    }
}
