using BirokratNext;
using BiroWoocommerceHub;
using BiroWoocommerceHubTests;
using core.customers.poledancerka;
using core.customers.poledancerka.mappers;
using core.customers.zgeneric;
using core.logic.mapping_woo_to_biro.document_insertion;
using Newtonsoft.Json;
using si.birokrat.next.common.build;
using System;
using System.Collections.Generic;
using System.IO;
using webshop_client_woocommerce;

namespace JsonIntegrationLoader {
    class Program {
        static void Main(string[] args) {

            string bironextAddress = "https://staging.birokrat.si";

            string content = File.ReadAllText(Path.Combine(Build.ProjectPath, "integration.json"));

            var some = JsonConvert.DeserializeObject<IntegrationJsonSpecification>(content);

            // parse biroclient
            string apikey = (string)some.BiroCredentials["ApiKey"];
            var biroClient = new ApiClientV2(bironextAddress, apikey);

            // parse wooclient
            string address = (string)some.WooCredentials["Address"];
            string ck = (string)some.WooCredentials["Ck"];
            string cs = (string)some.WooCredentials["Cs"];
            string version = (string)some.WooCredentials["Version"];
            var wooClient = new WooApiClient(new WoocommerceCaller_NetworkFailureGuard(5, new WoocommerceRESTPythonCaller(address, ck, cs, version)));

            new IntegrationFactory(biroClient, wooClient).ParseIntegration(some);
        }
    }
}