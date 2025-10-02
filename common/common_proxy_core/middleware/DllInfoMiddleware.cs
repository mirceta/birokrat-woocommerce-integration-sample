using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using si.birokrat.next.common.conversion;
using si.birokrat.next.common.serialization;
using si.birokrat.next.common_proxy_standard.models;
using System.Text;
using System.Threading.Tasks;

namespace si.birokrat.next.common_proxy_core.middleware {
    public class DllInfoMiddleware {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DllInfoMiddleware> _logger;
        private readonly bool _global;

        public DllInfoMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<DllInfoMiddleware> logger, bool global) {
            _next = next;
            _configuration = configuration;
            _logger = logger;
            _global = global;
        }

        public Task InvokeAsync(HttpContext context) {
            var route = context.Request.Path.Value;

            context.Request.Headers.TryGetValue("Authorization", out StringValues token);

            var info = new DllInfo {
                Token = token.Count > 0 ? token.ToString().Split(' ')[1] : string.Empty,
                Global = _global,
                TaxNumber = context.User.FindFirst("taxNumber")?.Value,
                UserName = context.User.FindFirst("userName")?.Value,
                DeviceIdentifier = context.User.FindFirst("deviceIdentifier")?.Value,
                Mode = _configuration["DLL"],
                SqlServer = _configuration["SqlServer"],
                PoslovnoLeto = _configuration["PoslovnoLeto"],
                Fiscalization = _configuration.GetValue<int>("Fiscalization")
            };

            if (!_global) {
                info.FilePath = _configuration["FilePath"];
            }

            context.Request.Headers.Add("info", HttpConverter.Encode(Serializer.ToJson(info), encoding: Encoding.UTF8));

            _logger.LogInformation($"[{route}]: Request forwarded.");

            return _next(context);
        }
    }
}
