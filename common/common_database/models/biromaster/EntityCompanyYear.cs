using System;
using System.Collections.Generic;
using System.Text;

namespace si.birokrat.next.common_database.models.biromaster {
    public partial class EntityCompanyYear {
        public int PkEntityCompanyYearId { get; set; }
        public int FkEntityCompanyId { get; set; }
        public DateTime CreatedDt { get; set; }
        public DateTime ModifiedDt { get; set; }
        public int Year { get; set; }
        public string YearCode { get; set; }
        public int LocalVersion { get; set; }
        public int RemoteVersion { get; set; }
        public int RemotePartnershipId { get; set; }
        public bool? IsActive { get; set; }
        public byte[] SyncTs { get; set; }

        public EntityCompany FkEntityCompany { get; set; }
    }
}
