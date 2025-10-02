using BironextWordpressIntegrationHub.structs;
using Newtonsoft.Json;

namespace BironextWordpressIntegrationHub {
    public class MShipping {
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

        public MShipping(Shipping shipping) {
            Name = shipping.FirstName + " " + shipping.LastName;
            Company = shipping.Company;
            Address = shipping.Address1;
            Address2 = shipping.Address2;
            City = shipping.City;
            State = shipping.State;
            Postcode = shipping.Postcode;
            Country = shipping.Country;
        }

        public MShipping() { }
    }
}
