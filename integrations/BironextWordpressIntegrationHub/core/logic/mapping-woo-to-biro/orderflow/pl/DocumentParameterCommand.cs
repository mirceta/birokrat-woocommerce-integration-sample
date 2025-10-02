using BironextWordpressIntegrationHub.structs;
using core.tools.wooops;
using gui_attributes;
using JsonIntegrationLoader.utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace core.logic.mapping_woo_to_biro.order_operations.pl {
    public class DocumentParameterCommand {

        public class Builder {

            
            public IDocumentParameterCommandCondition condition = null;
            public ParameterOperation operation;
            public string fieldName;
            public IDocumentParameterCommandValue value;
            public string replaceWith;

            public bool requiresRefresh;

            public Builder() {

            }

            public Builder SetValue(IDocumentParameterCommandValue value) {
                this.value = value;
                return this;
            }

            public Builder SetCondition(IDocumentParameterCommandCondition condition) {
                this.condition = condition;
                return this;
            }

            public Builder SetOperation(ParameterOperation operation) {
                this.operation = operation;
                return this;
            }

            public Builder SetFieldName(string fieldName) {
                this.fieldName = fieldName;
                return this;
            }

            public Builder SetReplaceWith(string value) {
                this.replaceWith = value;
                return this;
            }

            public DocumentParameterCommand Build() {

                // condition
                if (condition == null) {
                    condition = new Tautology();
                }

                // operation & replaceWith
                if (operation == ParameterOperation.REPLACE && replaceWith == null) {
                    throw new Exception("");
                }
                
                
                
                // fieldName
                if (string.IsNullOrEmpty(fieldName)) {
                    throw new Exception("");
                }
                
                // value
                
                if (fieldName == "cmbVrstaProdaje") {
                    requiresRefresh = true;
                }

                return new DocumentParameterCommand(this);
            }

        }

        [GuiConstructor]
        public DocumentParameterCommand(
        IDocumentParameterCommandCondition condition,
        ParameterOperation operation,
        string fieldName,
        IDocumentParameterCommandValue value,
        string replaceWith = null,
        bool requiresRefresh = false)
        {
            // condition
            this.condition = condition ?? new Tautology();

            // operation & replaceWith validation
            if (operation == ParameterOperation.REPLACE && replaceWith == null)
            {
                throw new ArgumentException("ReplaceWith cannot be null when operation is REPLACE.");
            }

            // fieldName validation
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException("FieldName cannot be null or empty.");
            }

            // value validation and other initializations
            this.operation = operation;
            this.fieldName = fieldName;
            this.value = value ?? throw new ArgumentException("Value function cannot be null.");
            this.replaceWith = replaceWith;

            // Specific field logic
            this.requiresRefresh = fieldName == "cmbVrstaProdaje" || requiresRefresh;
        }

        private DocumentParameterCommand(Builder builder) {
            condition = builder.condition;
            operation = builder.operation;
            fieldName = builder.fieldName;
            value = builder.value;
            replaceWith = builder.replaceWith;
            
            requiresRefresh = builder.requiresRefresh;
        }

        bool requiresRefresh;
        IDocumentParameterCommandCondition condition;
        ParameterOperation operation;
        string fieldName;
        IDocumentParameterCommandValue value;
        string replaceWith;
        public bool RequiresRefresh { get => requiresRefresh; }
        public IDocumentParameterCommandCondition Condition { get => condition; }
        public ParameterOperation Operation { get => operation; }
        public string FieldName { get => fieldName; }
        public IDocumentParameterCommandValue Value { get => value; }
        public string ReplaceWith { get => replaceWith; }
    }

    public interface IDocumentParameterCommandCondition {
        bool Is(WoocommerceOrder order, Dictionary<string, object> data);
    }

    public class Tautology : IDocumentParameterCommandCondition
    {
        public bool Is(WoocommerceOrder order, Dictionary<string, object> data)
        {
            return true;
        }
    }

    public class ShippingCountry : IDocumentParameterCommandCondition
    {

        List<string> acceptedCountryCodes;
        public ShippingCountry(List<string> acceptedCountryCodes) { 
            this.acceptedCountryCodes = acceptedCountryCodes;
        }
        public bool Is(WoocommerceOrder order, Dictionary<string, object> data)
        {
            return acceptedCountryCodes.Contains((string)data["wooshippingcountry"]);
        }
    }

    public class VatExempt : IDocumentParameterCommandCondition
    {
        public bool Is(WoocommerceOrder order, Dictionary<string, object> data)
        {
            return GWooOps.IsVatExempt(order);
        }
    }

    public class Not : IDocumentParameterCommandCondition
    {
        IDocumentParameterCommandCondition cond;
        public Not(IDocumentParameterCommandCondition cond) { 
            this.cond = cond;
        }
        public bool Is(WoocommerceOrder order, Dictionary<string, object> data)
        {
            return !cond.Is(order, data);
        }
    }

    public class And : IDocumentParameterCommandCondition
    {


        IDocumentParameterCommandCondition op1;
        IDocumentParameterCommandCondition op2;
        public And(IDocumentParameterCommandCondition op1, IDocumentParameterCommandCondition op2) {
            this.op1 = op1;
            this.op2 = op2;
        }

        public bool Is(WoocommerceOrder order, Dictionary<string, object> data)
        {
            return op1.Is(order, data) && op2.Is(order, data);
        }
    }

    public class And3 : IDocumentParameterCommandCondition
    {


        IDocumentParameterCommandCondition op1;
        IDocumentParameterCommandCondition op2;
        IDocumentParameterCommandCondition op3;
        public And3(IDocumentParameterCommandCondition op1, IDocumentParameterCommandCondition op2, IDocumentParameterCommandCondition op3)
        {
            this.op1 = op1;
            this.op2 = op2;
            this.op3 = op3;
        }

        public bool Is(WoocommerceOrder order, Dictionary<string, object> data)
        {
            return op1.Is(order, data) && op2.Is(order, data) && op3.Is(order, data);
        }
    }

    public interface IDocumentParameterCommandValue
    {
        string Get(WoocommerceOrder order, Dictionary<string, object> data);
    }

    public class Const : IDocumentParameterCommandValue
    {

        string value;
        public Const(string value) {
            this.value = value;
        }

        public string Get(WoocommerceOrder order, Dictionary<string, object> data)
        {
            return value;
        }
    }

    public class BiroShippingCountry : IDocumentParameterCommandValue
    {
        public string Get(WoocommerceOrder order, Dictionary<string, object> data)
        {
            return DetermineVrstaProdajeCountry(data);
        }

        static string DetermineVrstaProdajeCountry(Dictionary<string, object> data)
        {
            return (string)data["biroshippingcountry"];
        }
    }

    public class DaysFromNow : IDocumentParameterCommandValue
    {
        int days;
        public DaysFromNow(int days) {
            this.days = days;
        }
        public string Get(WoocommerceOrder order, Dictionary<string, object> data)
        {
            return DateTime.Now.AddDays(days).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }

    public class Template : IDocumentParameterCommandValue
    {
        private string template;

        public Template(string template)
        {
            this.template = template;
        }

        public string Get(WoocommerceOrder order, Dictionary<string, object> data)
        {
            string some = template + "";
            some = some.Replace("$$$ORDER_NUMBER$$$", order.Data.Number);
            some = ReplaceSubstringsWithDictionary(some, data);
            return some;
        }

        public string ReplaceSubstringsWithDictionary(string template, Dictionary<string, object> replacements)
        {
            string pattern = @"\{{3}(.*?)\}{3}";

            return Regex.Replace(template, pattern, match =>
            {
                // Extract the key from the match (excluding the braces)
                string key = match.Groups[1].Value;

                // Check if the key exists in the provided dictionary and use the value for replacement
                if (replacements.TryGetValue(key, out object replacementValue))
                {
                    return (string)replacementValue;
                }

                // Optionally, handle the case where there's no matching key in the dictionary
                // For now, return the match as-is or you could return a random string or any placeholder
                return match.Value; // Or alternatively, generate a random string or return a placeholder
            });
        }

    }

    public class Template2 : IDocumentParameterCommandValue
    {

        OrderAttributeTemplateParser2 parser;
        string outertemplate;
        public Template2(OrderAttributeTemplateParser2 parser, string outertemplate) {
            this.parser = parser;
            this.outertemplate = outertemplate; // [[[parser]]] will be replaced with parser output in outertemplate
        }

        public string Get(WoocommerceOrder order, Dictionary<string, object> data)
        {
            var parsed = parser.Parse(order);

            string some = outertemplate + "";
            some = outertemplate.Replace("[[[parser]]]", parsed);

            return new Template(some).Get(order, data);
        }
    }

}
