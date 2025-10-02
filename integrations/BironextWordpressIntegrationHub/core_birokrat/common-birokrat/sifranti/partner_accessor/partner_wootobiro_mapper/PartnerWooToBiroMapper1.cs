using BironextWordpressIntegrationHub.structs;
using core.logic.common_woo;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.logic
{
    public class PartnerWooToBiroMapper1 : IPartnerWooToBiroMapper {

        ICountryMapper countryMapper;
        IStatusZavezancaMapper statusZavezancaMapper;
        IStatusPartnerjaMapper statusPartnerjaMapper;
        public PartnerWooToBiroMapper1(ICountryMapper countryMapper,
            IStatusZavezancaMapper statusZavezancaMapper,
            IStatusPartnerjaMapper statusPartnerjaMapper) {
            
            if (statusPartnerjaMapper == null)
                throw new ArgumentNullException("statusPartnerjaMapper");
            if (statusPartnerjaMapper == null)
                throw new ArgumentNullException("statusPartnerjaMapper");
            if (countryMapper == null)
                throw new ArgumentNullException("countryMapper");

            this.countryMapper = countryMapper;
            this.statusZavezancaMapper = statusZavezancaMapper;
            this.statusPartnerjaMapper = statusPartnerjaMapper;
        }

        public async Task<string> GetUlica(WoocommerceOrder order) {
            return (order.Data.Billing.Address1 + " " + order.Data.Billing.Address2).Trim();
        }
        public async Task<string> GetNaziv(WoocommerceOrder order) {
            return (order.Data.Billing.FirstName + " " + order.Data.Billing.LastName).Replace("'", "");
        }

        public async Task<string> GetDodatekNaziva(WoocommerceOrder order) {
            return order.Data.Billing.Company;
        }

        public async Task<Dictionary<string, object>> GetPackedParameters(WoocommerceOrder order) {
            var pack = new Dictionary<string, object>();
            pack["txtNazivPartnerja"] = await GetNaziv(order);
            pack["txtUlica"] = await GetUlica(order);
            pack["KrajPosta"] = order.Data.Billing.Postcode;
            pack["KrajNaziv"] = order.Data.Billing.City;
            if (countryMapper != null)
                pack["cmbDrzava"] = await countryMapper.Map(order.Data.Billing.Country);
            else
                pack["cmbDrzava"] = order.Data.Billing.Country;
            pack["txtemail"] = order.Data.Billing.Email;
            pack["txtTelefon"] = order.Data.Billing.Phone;
            pack["txtDodatekNaziva"] = await GetDodatekNaziva(order);

            pack["StatusZavezanca"] = await statusZavezancaMapper.GetStatusZavezanca(order);
            pack["StatusOsebe"] = await statusPartnerjaMapper.GetStatusPartnerja(order);

            return pack;
        }
    }


    public interface IStatusZavezancaMapper {
        Task<string> GetStatusZavezanca(WoocommerceOrder order);
    }

    public interface IStatusPartnerjaMapper {
        Task<string> GetStatusPartnerja(WoocommerceOrder order);
    }

    public class B2CStatusPartnerjaMapper : IStatusPartnerjaMapper, IStatusZavezancaMapper
    {
        public B2CStatusPartnerjaMapper() { }

        public async Task<string> GetStatusPartnerja(WoocommerceOrder order) {
            return "Fizična oseba";
        }

        public async Task<string> GetStatusZavezanca(WoocommerceOrder order) {
            return "Končni potrošnik";
        }
    }

    public class DancerkaB2BStatusPartnerjaMapper : IStatusPartnerjaMapper, IStatusZavezancaMapper
    {
        IVatIdParser vatParser;

        public DancerkaB2BStatusPartnerjaMapper(IVatIdParser vatParser) {
            this.vatParser = vatParser;
        }

        public async Task<string> GetStatusPartnerja(WoocommerceOrder order) {
            string vat = await vatParser.Get(order);
            if (vat.Any(x => char.IsLetter(x))) {
                return "Zavezanec za DDV";
            } else {
                return "Končni potrošnik";
            }
        }

        public async Task<string> GetStatusZavezanca(WoocommerceOrder order) {
            string vat = await vatParser.Get(order);
            if (vat.Any(x => char.IsLetter(x))) {
                return "Pravna oseba";
            } else {
                return "Fizična oseba";
            }
        }
    }

    public class EstradaStatusPartnerjaMapper : IStatusZavezancaMapper, IStatusPartnerjaMapper
    {
        IVatIdParser vatParser;
        public EstradaStatusPartnerjaMapper(IVatIdParser vatParser) {
            this.vatParser = vatParser;
        }

        public async Task<string> GetStatusPartnerja(WoocommerceOrder order) {
            string vat = await vatParser.Get(order);
            if (vat.Any(x => char.IsLetter(x)) && !vat.Contains("OIB")) {
                return "Zavezanec za DDV";
            } else {
                return "Končni potrošnik";
            }
        }

        public async Task<string> GetStatusZavezanca(WoocommerceOrder order) {
            string vat = await vatParser.Get(order);
            if (vat.Any(x => char.IsLetter(x)) && !vat.Contains("OIB")) {
                return "Pravna oseba";
            } else {
                return "Fizična oseba";
            }
        }
    }
}
