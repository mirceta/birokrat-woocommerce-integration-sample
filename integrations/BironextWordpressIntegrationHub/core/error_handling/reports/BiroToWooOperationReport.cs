using core.logic.common_birokrat;
using System;
using System.Collections.Generic;
using System.Text;

namespace biro_to_woo_common.error_handling.reports
{
    public class BiroToWooOperationReport : IOperationReport
    {
        string skuAttribute;
        string skuAttributeValue;
        string varAttribute;
        string varAttributeValue;

        Dictionary<string, object> biroItemUnderInspection;
        OperationOutcome outcome;
        Exception ex;
        
        /*
         skuField and varField are inside because they comprise the signature of the operation report. No other reason.
         */
        public BiroToWooOperationReport(BirokratField skuField,
            BirokratField varField,
            Dictionary<string, object> biroItem,
            OperationOutcome outcome,
            Exception ex)
        {
            Initialize(skuField, varField, biroItem, outcome, ex);
        }

        public BiroToWooOperationReport(Dictionary<string, object> biroItem,
            OperationOutcome outcome) {
            Initialize(BirokratField.SifraArtikla, BirokratField.None, biroItem, outcome, null);
        }

        private void Initialize(BirokratField skuField,
            BirokratField varField,
            Dictionary<string, object> biroItem,
            OperationOutcome outcome,
            Exception ex) {
            this.biroItemUnderInspection = biroItem;
            this.outcome = outcome;
            this.ex = ex;


            string skuAttr = BirokratNameOfFieldInFunctionality.SifrantArtiklov(skuField);
            skuAttribute = skuAttr;
            skuAttributeValue = biroItem[skuAttr] as string;


            string varAttr = "";
            if (varField != BirokratField.None) {
                varAttr = BirokratNameOfFieldInFunctionality.SifrantArtiklov(varField);
                varAttribute = varAttr;
                varAttributeValue = biroItem[varAttr] as string;
            }
        }
        
        public object ObjectUnderInspection => biroItemUnderInspection;

        public string Id => skuAttributeValue;

        public string Signature => $"{skuAttribute}:{skuAttributeValue}:{varAttribute}:{varAttributeValue}";

        public OperationOutcome OperationOutcome => outcome;
        

        public Exception Ex => ex;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("----------------------------------------------");
            sb.AppendLine("<black>");
            sb.AppendLine("<bold>");
            sb.AppendLine("Operation Report Summary:");
            sb.AppendLine("<black>");
            sb.AppendLine($"Operation Outcome: {OperationOutcome}");
            sb.AppendLine("");
            sb.AppendLine("<bold>");
            sb.AppendLine("Product Details:");
            sb.AppendLine("<black>");
            sb.AppendFormat("SKU Attribute: {0}, SKU Value: {1}\n", skuAttribute, skuAttributeValue);

            sb.AppendLine("");
            if (!string.IsNullOrEmpty(varAttribute))
            {
                sb.AppendLine("<gray>");
                sb.AppendFormat("Variant Attribute: {0}, Variant Value: {1}\n", varAttribute, varAttributeValue);
                sb.AppendLine("");
            }

            sb.AppendLine("");
            sb.AppendLine("<bold>");
            sb.AppendLine("Item Details:");
            sb.AppendLine("<gray>");
            foreach (var item in biroItemUnderInspection)
            {
                sb.AppendFormat("{0}: {1}\n", item.Key, item.Value);
            }


            sb.AppendLine("");
            if (Ex != null)
            {
                sb.AppendLine("<bold>");
                sb.AppendLine("Error:");
                sb.AppendLine("<black>");
                sb.AppendLine(Ex.Message);

                sb.AppendLine("<gray>");
                sb.AppendLine("Technical Details:");
                sb.AppendLine(Ex.ToString());
                sb.AppendLine("----------------------------------------------");
            }
            sb.AppendLine("");
            sb.AppendLine("");

            return sb.ToString();
        }
    }
}
