

using System;
using System.Collections.Generic;
using BirokratNext;
using BiroWoocommerceHub;
using BiroWoocommerceHub.structs_wc_to_biro;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.customers.spicasport;
using core.logic.common_birokrat;
using core.logic.common_woo;
using core.logic.mapping_biro_to_woo;
using core.logic.mapping_biro_to_woo.change_handlers;
using core.structs;
using core.tools.zalogaretriever;
using tests.tools;
using webshop_client_woocommerce;
using BiroWoocommerceHubTests.tools;
using core;
using core.tools.attributemapper;
using core.zgeneric;
using core.customers;
using core.customers.zgeneric;
using core.logic.mapping_woo_to_biro.document_insertion;
using BiroWoocommerceHub.logic;
using core.customers.zgeneric.order_operations;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_additions;
using allintegrations.customers;
using core.logic.mapping_woo_to_biro.order_operations;
using core.logic.mapping_woo_to_biro.document_insertion.postavke_extractors;
using core.logic.mapping_woo_to_biro;
using gui_inferable;
using core.customers.poledancerka.mappers;



using core.logic.mapping_woo_to_biro.product_ops;
using ApiClient.utils;

using System.Reflection;
using core.logic.mapping_woo_to_biro.order_operations.pl;

namespace core
{
    public class Mock : IMockWithInject<LazyIntegration>
    {

        IApiClientV2 iapiclientv2 = null;
        IOutApiClient ioutapiclient = null;
        IZalogaRetriever izalogaretriever = null;
        IVatIdParser ivatidparser = null;
        ICountryMapper icountrymapper = null;

        public Mock()
        {
            iapiclientv2 = new ApiClientV2(
@"https://next.birokrat.si/api/",
@"yGr6iFO38ns3rY23zR82Ae6AeYMRcqTeynvpOPaVdaR=",
3600);
            ioutapiclient = new WooApiClient(
            new WoocommerceCaller_NetworkFailureGuard(
            5,
            new WoocommerceRESTPythonCaller(
            @"https://smileconceptstore.si/",
            @"ck_4281dbc995d4706e8664c9702abbb81c3cf48505",
            @"cs_4da317296a4bea72b454456f0052410f5fac83da",
            @"wc/v3",
            2,
            @"")));
            izalogaretriever = null;
            ivatidparser = new EstradaVatIdParser(
            iapiclientv2);
            icountrymapper = new WooStaticCountryMapper(
            new Dictionary<String, String>() {
{ "SI",@"SLO" }
,{ "HR",@"CRO" }
            }
            ,
            @"SLO");
        }

        public void Inject(Dictionary<string, object> injections)
        {
            FieldInfo[] fieldInfos = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var x in injections.Keys)
            {
                foreach (var fieldInfo in fieldInfos)
                {
                    if (x == fieldInfo.Name)
                    {
                        fieldInfo.SetValue(this, injections[x]);
                    }
                }
            }
        }

        public void SetFieldProperty(string fieldName, string propertyName, object value)
        {
            // Get the FieldInfo of the field we want to access.
            FieldInfo fieldInfo = this.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
            {
                throw new ArgumentException($"No field named {fieldName} found", nameof(fieldName));
            }

            // Get the current value of the field. We expect it to be an object.
            object fieldValue = fieldInfo.GetValue(this);
            if (fieldValue == null)
            {
                throw new NullReferenceException($"Field {fieldName} is null");
            }

            // Get the PropertyInfo of the property we want to set.
            PropertyInfo propertyInfo = fieldValue.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"No property named {propertyName} found in {fieldName}", nameof(propertyName));
            }

            // Set the value of the property.
            propertyInfo.SetValue(fieldValue, value);
        }

        public Dictionary<string, object> GetFields()
        {
            Dictionary<string, object> fieldsDictionary = new Dictionary<string, object>();
            FieldInfo[] fieldInfos = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var fieldInfo in fieldInfos)
            {
                // Include a check here if you want to exclude certain fields
                // For instance, if you don't want to include fields of certain types, you could do so.

                // Retrieve the value of the field for 'this' instance
                object fieldValue = fieldInfo.GetValue(this);

                // Add the field name and value to the dictionary
                fieldsDictionary.Add(fieldInfo.Name, fieldValue);
            }

            return fieldsDictionary;
        }

        public LazyIntegration Get()
        {
            return new LazyIntegration()
            {
                Name = "HISAVIZIJ_WOOTOBIRO_PRODUCTION",
                Type = "BIROTOWOO",
                Key = "",
                BuildIntegrationAsync = async () => {
                    return new RegularIntegration(
iapiclientv2,
ioutapiclient,
@"HISAVIZIJ_WOOTOBIRO_PRODUCTION",
null,
new OrderFlowProductInserterWooToBiro(
new OrderFlow(
iapiclientv2,
new SwitchOnDavcnaPartnerInserter(
iapiclientv2,
ivatidparser,
new PartnerWooToBiroMapper1(
icountrymapper,
new EstradaStatusPartnerjaMapper(
ivatidparser),
new EstradaStatusPartnerjaMapper(
ivatidparser)),
true),
new List<ConditionOperationPair>() {
new ConditionOperationPair(
new OrderCondition(
new List<String>() {
@"processing"
,@"on-hold"
}
,
new List<String>() {
}
,
false,
@""),
new DocumentInsertionOrderOperationCR(
new DocumentInsertion(
iapiclientv2,
ApiClient.utils.BirokratDocumentType.DOBAVNICA,
new BirokratAttributeIsOriginalOrVariationSku_BirokratPostavkaExtractor(
new BirokratPostavkaUtils(
false,
true)),
new List<IAdditionalOperationOnPostavke>() {
new CommentAddOriginProductSku_PostavkaAddOp(
false)
,new CouponPercent_PostavkeAddOp(
)
,new Shipping_PostavkaAddOp(
iapiclientv2,
@"6     0 DDV oproščen promet            Storitev",
new List<IAdditionalOperationOnPostavke>() {
}
,
@"")
,new ConditionalAddExpense_PostavkaAddOp(
iapiclientv2,
new PaymentMethod_OrderAddOpCondition(
@"cod"),
@"Strošek",
@"1.00",
@"6     0 DDV oproščen promet            Storitev")
,new PriceMultiplier_PostavkaAddOp(
new BirokratPostavkaUtils(
false,
true),
0.13272280841462605,
new Currency_OrderAddOpCondition(
@"HRK"))
}
,
icountrymapper),
ApiClient.utils.BirokratDocumentType.DOBAVNICA,
new DocumentParametersModifierOrderOperationCR(
iapiclientv2,
new List<DocumentParameterCommand>() {
new DocumentParameterCommand(
new Tautology(
),
core.logic.mapping_woo_to_biro.order_operations.pl.ParameterOperation.SET,
@"DatumValute",
new DaysFromNow(
7),
@"",
false)
,new DocumentParameterCommand(
new Tautology(
),
core.logic.mapping_woo_to_biro.order_operations.pl.ParameterOperation.SET,
@"DatumOdpreme",
new Const(
@""),
@"",
false)
}
,
icountrymapper,
new SaveDocumentOrderOperationCR(
iapiclientv2,
null,
@"C:\Users\Administrator\Desktop\playground\bironext-woocommerce-integration\BironextWordpressIntegrationHub\gui_generator_runner\appdata\hisavizij"))))
}
,
new List<ConditionAttachmentPair>()
{
}
),
null),
new Options()
);
                }
            };
        }

        public string GetSignature()
        {
            return "BIROTOWOO:HISAVIZIJ_WOOTOBIRO_PRODUCTION";
        }
    }
}
