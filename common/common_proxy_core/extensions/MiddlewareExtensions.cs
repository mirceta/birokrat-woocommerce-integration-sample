using Microsoft.AspNetCore.Builder;
using si.birokrat.next.common_proxy_core.middleware;

namespace si.birokrat.next.common_proxy_core.extensions {
    public static class MiddlewareExtensions {
        public static IApplicationBuilder UseAuthorizationMiddleware(this IApplicationBuilder builder) {
            return builder.UseMiddleware<AuthorizationMiddleware>();
        }

        public static IApplicationBuilder UseDllInfoMiddleware(this IApplicationBuilder builder, bool global) {
            return builder.UseMiddleware<DllInfoMiddleware>(global);
        }

        public static IApplicationBuilder UseDataInfoMiddleware(this IApplicationBuilder builder) {
            return builder.UseMiddleware<DataInfoMiddleware>();
        }
    }
}
