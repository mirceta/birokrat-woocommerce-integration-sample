using BironextWordpressIntegrationHub.structs;
using birowoo_exceptions;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonIntegrationLoader.utils {
    public class OrderAttributeTemplateParser {

        public static string Parse(string template, WoocommerceOrder x) {
            template = template.Replace("$$$ORDER_ID$$$", GWooOps.SerializeIntWooProperty(x.Data.Id) + "");
            template = template.Replace("$$$ORDER_NUMBER$$$", GWooOps.SerializeIntWooProperty(x.Data.Number) + "");
            return template;
        }

        public static string ParseFromOrderDocumentPLOperation(string template, Dictionary<string, object> data) {
            template = template.Replace("$$$ORDER_ID$$$", (string)data["orderId"]);
            template = template.Replace("$$$ORDER_NUMBER$$$", (string)data["orderNumber"]);
            return template;
        }

    }

    public interface IOrderAttributeTemplateParser
    {
        string Parse(string template, WoocommerceOrder x);
        string ParseFromOrderDocumentPLOperation(string template, Dictionary<string, object> data);
    }

    public class OrderAttributeTemplateParser2 {

        string template;
        IOrderAttributeTemplateParser orderAttributeTemplateParser;

        public OrderAttributeTemplateParser2(string template, IOrderAttributeTemplateParser orderAttributeTemplateParser = null)
        {
            this.template = template;
            if (orderAttributeTemplateParser == null)
            {
                this.orderAttributeTemplateParser = new OrderAttributeTemplateParserImplementation();
            }
            else
            {
                this.orderAttributeTemplateParser = orderAttributeTemplateParser;
            }
        }

        public string Parse(WoocommerceOrder order) { 
            return orderAttributeTemplateParser.Parse(template, order);
        }

        public string ParseFromOrderDocumentPLOperation(Dictionary<string, object> data) {
            return orderAttributeTemplateParser.ParseFromOrderDocumentPLOperation(template, data);
        }
    }

    public class OrderAttributeTemplateParserImplementation : IOrderAttributeTemplateParser
    {
        public string Parse(string template, WoocommerceOrder x)
        {
            return OrderAttributeTemplateParser.Parse(template, x);
        }

        public string ParseFromOrderDocumentPLOperation(string template, Dictionary<string, object> data)
        {
            return OrderAttributeTemplateParser.ParseFromOrderDocumentPLOperation(template, data);
        }
    }

    public class OrderAttributeTemplateParserDecorator : IOrderAttributeTemplateParser
    {
        private readonly IOrderAttributeTemplateParser decorated;
        private readonly Dictionary<string, string> paymentMethodMapper;

        public OrderAttributeTemplateParserDecorator(Dictionary<string, string> paymentMethodMapper, IOrderAttributeTemplateParser decorated = null)
        {
            if (decorated == null)
            {
                this.decorated = new OrderAttributeTemplateParserImplementation();
            }
            else
            {
                this.decorated = decorated;
            }
            this.paymentMethodMapper = paymentMethodMapper;
        }

        public string Parse(string template, WoocommerceOrder x)
        {
            template = decorated.Parse(template, x);
            if (paymentMethodMapper.TryGetValue(x.Data.PaymentMethod, out string paymentMethod))
            {
                template = template.Replace("$$$PAYMENT_METHOD$$$", paymentMethod.ToString());
            }
            else {
                throw new IntegrationProcessingException($"The mapping of payment method '{x.Data.PaymentMethod}' to a number was not defined");
            }
            return template;
        }

        public string ParseFromOrderDocumentPLOperation(string template, Dictionary<string, object> data)
        {
            // This operation does not deal with payment method, so just delegate to decorated object
            return decorated.ParseFromOrderDocumentPLOperation(template, data);
        }
    }
}
