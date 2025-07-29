using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompliGenie.Models;
using CompliGenie.Services.Interfaces;

namespace CompliGenie.Services
{
    public class MockTenantRepository : ITenantRepository
    {
        private readonly List<Tenant> _tenants = new();

        public Task<Tenant> CreateAsync(Tenant tenant)
        {
            tenant.Id = Guid.NewGuid();
            tenant.CreatedAt = DateTime.UtcNow;
            _tenants.Add(tenant);
            return Task.FromResult(tenant);
        }

        public Task<Tenant?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(_tenants.FirstOrDefault(t => t.Id == id));
        }

        public Task<Tenant?> GetBySubdomainAsync(string subdomain)
        {
            return Task.FromResult(_tenants.FirstOrDefault(t => t.Subdomain == subdomain));
        }

        public Task<bool> ExistsAsync(string companyName)
        {
            return Task.FromResult(_tenants.Any(t => t.Name.Equals(companyName, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<Tenant> UpdateAsync(Tenant tenant)
        {
            var existing = _tenants.FirstOrDefault(t => t.Id == tenant.Id);
            if (existing != null)
            {
                _tenants.Remove(existing);
                _tenants.Add(tenant);
            }
            return Task.FromResult(tenant);
        }
    }
}
