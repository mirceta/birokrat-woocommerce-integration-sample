using BirokratNext;
using BironextWordpressIntegrationHub;
using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub;
using BiroWoocommerceHub.logic.eslog_gen;
using BiroWoocommerceHub.structs_wc_to_biro;
using core.logic.mapping_woo_to_biro.document_insertion;
using core.tools.birokratops;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace core.logic.eslog_gen {
    public class EslogGen {

        public static string CreateXML(WoocommerceOrder order, List<BirokratPostavka> postavke, string additionalNumber, string externalUniqueIdentifier = "") {
            string strpostavke = InvoiceSpecification.Get(postavke);
            OrderSpecification spec = new OrderSpecification() {
                order_date = order.Data.DateCreated.Date,
                billing_city = order.Data.Billing.City,
                billing = order.Data.Billing,
                shipping = order.Data.Shipping
            };
            return BuildXML(spec, strpostavke, additionalNumber, externalUniqueIdentifier);
        }

        
        public static string CreateXMLWithPricesAndDiscounts(WoocommerceOrder order, List<BirokratPostavka> postavke, string additionalNumber, string externalUniqueIdentifier = "") {
            string strpostavke = InvoiceSpecification.GetWithPricesAndDiscounts(postavke);
            OrderSpecification spec = new OrderSpecification() {
                order_date = order.Data.DateCreated.Date,
                billing_city = order.Data.Billing.City,
                billing = order.Data.Billing,
                shipping = order.Data.Shipping
            };
            return BuildXML(spec, strpostavke, additionalNumber, externalUniqueIdentifier);
        }

        private static string BuildXML(OrderSpecification order, string postavke, string additionalNumber, string externalUniqueIdentifier = "") {
            string xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<?xml-stylesheet type='text/xsl' href='http://vizualiziraj.si/eInvoiceVizualization_20110530.xslt'?>
<IzdaniRacunEnostavni xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" xmlns:xds=""http://uri.etsi.org/01903/v1.1.1#"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:noNamespaceSchemaLocation=""http://www.gzs.si/e-poslovanje/sheme/eSlog_1-6_EnostavniRacun.xsd"">
    <Racun Id=""data"">
        {Preamble.Get(order.order_date,
                additionalNumber,
                order.billing_city,
                externalUniqueIdentifier)}
        {PartnerDetails.Preamble()}
        {PartnerDetails.Billing(order.billing)}
        {PartnerDetails.Shipping(order.shipping)}
        {postavke}
        {Postamble.Get()}
    </Racun>
</IzdaniRacunEnostavni>
            ";
            return xml;
        }

        private static string VrstaZneskaPostavke() {
            // 38 koncni znesek -> 
            // 52 znesek s popustom
            // 125 obdavcljivi znesek
            // 203 znesek s popustom in kaznijo -> znesek brez davka
            return "";
        }
    }
}
