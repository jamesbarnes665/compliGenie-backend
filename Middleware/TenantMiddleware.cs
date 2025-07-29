using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;
using CompliGenie.Data;
using Microsoft.EntityFrameworkCore;

namespace CompliGenie.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _excludedPaths = new[] { 
            "/api/health", 
            "/api/setup",
            "/swagger", 
            "/swagger/index.html" 
        };

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // Skip authentication for excluded paths
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (_excludedPaths.Any(excludedPath => path.StartsWith(excludedPath)))
            {
                await _next(context);
                return;
            }

            var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(apiKey))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("API key required");
                return;
            }

            // In production, hash the API key before comparing
            var tenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.ApiKeyHash == apiKey);

            if (tenant == null)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Invalid API key");
                return;
            }

            CurrentTenant.Id = tenant.Id;
            context.Items["TenantId"] = tenant.Id;
            context.Items["Tenant"] = tenant;

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
