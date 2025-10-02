using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace si.birokrat.next.common_proxy_core.middleware {
    public class AuthorizationMiddleware {
        private readonly RequestDelegate _next;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthorizationMiddleware> _logger;

        public AuthorizationMiddleware(
            RequestDelegate next,
            IAuthorizationService authorizationService,
            IConfiguration configuration,
            ILogger<AuthorizationMiddleware> logger) {
            _next = next;
            _authorizationService = authorizationService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context) {
            var route = context.Request.Path.Value;

            context.Request.Headers.Add("proxystart_time", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff"));
            
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            var result = await _authorizationService.AuthorizeAsync(context.User, policy);
            if (!result.Succeeded) {
                _logger.LogWarning($"[{route}]: User is unauthorized.");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            } else {
                var taxNumber = context.User.FindFirst("taxNumber")?.Value;
                var userName = context.User.FindFirst("userName")?.Value;
                var user = string.IsNullOrEmpty(taxNumber) ? $"User \"{userName}\"" : $"Company \"{taxNumber}\" | user \"{userName}\"";
                _logger.LogInformation($"[{route}]: {user} successfully authorized.");
                await _next(context);
            }
        }
    }
}
