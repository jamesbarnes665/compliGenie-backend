using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

using CompliGenie.Services;
using CompliGenie.Context;

namespace CompliGenie.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                // Extract API key from headers
                var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("Request received without API key from {IpAddress}", 
                        context.Connection.RemoteIpAddress);
                    
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":\"API key required\"}");
                    return;
                }

                // Get tenant service from DI
                var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
                var tenant = await tenantService.GetByApiKeyAsync(apiKey);
                
                if (tenant == null)
                {
                    _logger.LogWarning("Invalid API key attempted: {ApiKey} from {IpAddress}", 
                        apiKey.Substring(0, Math.Min(apiKey.Length, 8)) + "...", 
                        context.Connection.RemoteIpAddress);
                    
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":\"Invalid API key\"}");
                    return;
                }

                // Set tenant context
                context.Items["TenantId"] = tenant.Id;
                context.Items["TenantName"] = tenant.Name;
                
                // Set current tenant in thread-safe context
                var currentTenant = context.RequestServices.GetRequiredService<ICurrentTenant>();
                currentTenant.Id = tenant.Id;
                currentTenant.Name = tenant.Name;
                currentTenant.StripeAccountId = tenant.StripeAccountId;
                
                // Add tenant info to response headers for debugging (only in development)
                var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
                if (env.IsDevelopment())
                {
                    context.Response.Headers.Append("X-Tenant-Id", tenant.Id.ToString());
                }

                // Log successful authentication
                _logger.LogInformation("Request authenticated for tenant {TenantName} ({TenantId})", 
                    tenant.Name, tenant.Id);

                // Continue to next middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in tenant middleware");
                throw;
            }
            finally
            {
                // Log performance metrics
                var duration = DateTime.UtcNow - startTime;
                if (duration.TotalMilliseconds > 5)
                {
                    _logger.LogWarning("Tenant middleware took {Duration}ms, exceeding 5ms threshold", 
                        duration.TotalMilliseconds);
                }
            }
        }
    }

    // Extension method to easily add middleware to pipeline
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
}

