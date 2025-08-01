using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CompliGenie.Services;
using CompliGenie.Context;
using CompliGenie.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CompliGenie.Tests.E2E
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Add test services
                services.AddSingleton<ITenantService, MockTenantService>();
                services.AddScoped<ICurrentTenant, CurrentTenant>();
                services.AddHttpContextAccessor();
                
                // Add mock controller for testing
                services.AddControllers();
            });

            builder.Configure(app =>
            {
                app.UseRouting();
                app.UseTenantMiddleware();
                app.UseAuthorization();
                
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    
                    // Add a simple test endpoint
                    endpoints.MapGet("/api/policies", async context =>
                    {
                        var currentTenant = context.RequestServices.GetRequiredService<ICurrentTenant>();
                        await context.Response.WriteAsync($"Tenant: {currentTenant.Name}");
                    });
                });
            });
        }
    }
}
