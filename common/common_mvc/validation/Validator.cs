using Microsoft.AspNetCore.Http;
using si.birokrat.next.common.serialization;
using System;
using System.IO;
using System.Text;

namespace si.birokrat.next.common_mvc.validation {
    public static class Validator {
        public static bool HasProperty(HttpRequest request, string propertyName) {
            request.Body.Position = 0;
            StreamReader reader = new StreamReader(request.Body, Encoding.UTF8);
            string body = reader.ReadToEnd();
            var obj = Serializer.FromJsonAnonymous(body);
            return obj.GetValue(propertyName, StringComparison.OrdinalIgnoreCase) != null;
        }
    }
}
