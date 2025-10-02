using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace si.birokrat.next.common_database.models.biro {
    public class PostnaKnjiga {
        [Key]
        public int Recno { get; set; }
        public DateTime Datum { get; set; }
    }
}
