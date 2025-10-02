using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace si.birokrat.next.common_database.models {
    public class Company : IdentityUser<int> {
        [Required]
        [StringLength(8, MinimumLength = 8)]
        public override string UserName { get; set; } // Tax number

        [Required]
        [StringLength(256)]
        public string CompanyName { get; set; }
    }
}
