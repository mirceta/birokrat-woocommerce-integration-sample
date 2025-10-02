using BironextWordpressIntegrationHub.structs;
using Newtonsoft.Json;

namespace BironextWordpressIntegrationHub {
    public class MBilling {

        [JsonProperty("BirokratId")]
        public string BirokratId { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("Company")]
        public string Company { get; set; }

        [JsonProperty("Address")]
        public string Address { get; set; }

        [JsonProperty("Address2")]
        public string Address2 { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("State")]
        public string State { get; set; }

        [JsonProperty("Postcode")]
        public string Postcode { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Phone")]
        public string Phone { get; set; }


        public MBilling(Billing billing, string BirokratId = null) {
            if (BirokratId != null) {
                this.BirokratId = BirokratId;
                Name = "";
                Company = "";
                Address = "";
                Address2 = "";
                City = "";
                State = "";
                Postcode = "";
                Country = "";
                Email = "";
                Phone = "";
            } else {
                Name = billing.FirstName + " " + billing.LastName;
                Company = billing.Company;
                Address = billing.Address1;
                Address2 = billing.Address2;
                City = billing.City;
                State = billing.State;
                Postcode = billing.Postcode;
                Country = billing.Country;
                Email = billing.Email;
                Phone = billing.Phone;
            }
        }
        public MBilling() { }
    }

}
