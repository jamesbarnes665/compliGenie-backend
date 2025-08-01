using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;
using CompliGenie.Services;
using CompliGenie.Services.Interfaces;

namespace CompliGenie.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
        {
            var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
            if (string.IsNullOrEmpty(apiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Missing API Key");
                return;
            }
            
            var tenant = await tenantService.GetByApiKeyAsync(apiKey);
            if (tenant == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }
            
            context.Items["TenantId"] = tenant.Id;
            context.Items["Tenant"] = tenant;
            
            // Set the current tenant for the request
            CurrentTenant.Id = tenant.Id;
            
            await _next(context);
        }
    }

    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
}
