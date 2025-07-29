using System;
using System.Threading.Tasks;
using CompliGenie.Models;

namespace CompliGenie.Services
{
    public class MockTenantService : ITenantService
    {
        public Task<Tenant?> GetByApiKey(string apiKey)
        {
            if (apiKey == "valid-key")
            {
                return Task.FromResult<Tenant?>(new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = "MockTenant"
                });
            }

            return Task.FromResult<Tenant?>(null);
        }
    }
}
