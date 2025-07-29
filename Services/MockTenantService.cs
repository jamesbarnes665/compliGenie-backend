using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompliGenie.Models;
using Microsoft.Extensions.Logging;

namespace CompliGenie.Services
{
    public class MockTenantService : ITenantService
    {
        private readonly Dictionary<string, Tenant> _tenants = new();
        private readonly ILogger<MockTenantService> _logger;

        public MockTenantService(ILogger<MockTenantService> logger)
        {
            _logger = logger;
            InitializeTestTenants();
        }

        public Task<Tenant?> GetByApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return Task.FromResult<Tenant?>(null);
            }

            // In a real implementation, you would hash the apiKey and compare with ApiKeyHash
            // For mock, we'll do a simple lookup
            var tenant = _tenants.Values.FirstOrDefault(t => t.ApiKeyHash == apiKey);
            
            if (tenant != null)
            {
                _logger.LogDebug("Found tenant {TenantName} for API key", tenant.Name);
            }
            else
            {
                _logger.LogDebug("No tenant found for API key");
            }
            
            return Task.FromResult(tenant);
        }

        public void AddTenant(Tenant tenant)
        {
            if (tenant == null || string.IsNullOrEmpty(tenant.ApiKeyHash))
            {
                throw new ArgumentException("Tenant and API key hash are required");
            }

            _tenants[tenant.ApiKeyHash] = tenant;
            _logger.LogInformation("Added test tenant {TenantName}", tenant.Name);
        }

        private void InitializeTestTenants()
        {
            var testTenants = new[]
            {
                new Tenant
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Demo Legal Platform",
                    Subdomain = "demo-legal",
                    ApiKeyHash = "demo-api-key-legal-12345", // In production, this would be hashed
                    StripeAccountId = "acct_demo_legal",
                    Settings = "{\"industry\":\"legal\",\"revenueSplit\":0.5,\"isTestAccount\":true}",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Tenant
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Demo Healthcare Platform",
                    Subdomain = "demo-health",
                    ApiKeyHash = "demo-api-key-health-67890", // In production, this would be hashed
                    StripeAccountId = "acct_demo_health",
                    Settings = "{\"industry\":\"healthcare\",\"revenueSplit\":0.5,\"isTestAccount\":true}",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }
            };

            foreach (var tenant in testTenants)
            {
                _tenants[tenant.ApiKeyHash] = tenant;
            }

            _logger.LogInformation("Initialized {Count} test tenants", testTenants.Length);
        }
    }
}
