using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using si.birokrat.next.common.conversion;
using si.birokrat.next.common.serialization;
using si.birokrat.next.common_proxy_standard.models;
using System.Text;
using System.Threading.Tasks;

namespace si.birokrat.next.common_proxy_core.middleware {
    public class DataInfoMiddleware {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataInfoMiddleware> _logger;

        public DataInfoMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<DataInfoMiddleware> logger) {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public Task InvokeAsync(HttpContext context) {
            var route = context.Request.Path.Value;

            var info = new DataInfo {
                TaxNumber = context.User.FindFirst("taxNumber")?.Value,
                AccountId = TypeConverter.StringToInteger(context.User.FindFirst("accountId")?.Value),
                ApplicationId = TypeConverter.StringToInteger(context.User.FindFirst("applicationId")?.Value),
                SqlServer = _configuration["SqlServer"],
                SqlUsername = _configuration["SqlUsername"],
                SqlPassword = _configuration["SqlPassword"]
            };

            context.Request.Headers.Add("info", HttpConverter.Encode(Serializer.ToJson(info), encoding: Encoding.UTF8));

            _logger.LogInformation($"[{route}]: Request forwarded.");

            return _next(context);
        }
    }
}
