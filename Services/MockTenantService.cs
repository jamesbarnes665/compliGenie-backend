using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using CompliGenie.Models;
using CompliGenie.Services.Interfaces;

namespace CompliGenie.Services
{
    public class MockTenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;

        public MockTenantService(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<Tenant?> GetByApiKeyAsync(string apiKey)
        {
            // In production, this would query the database
            // For now, we hash the API key and look it up
            var apiKeyHash = HashApiKey(apiKey);
            
            // This is a mock implementation - in production this would be a DB query
            var allTenants = await _tenantRepository.GetAllAsync();
            return allTenants.FirstOrDefault(t => t.ApiKeyHash == apiKeyHash);
        }

        public async Task<Tenant?> GetByIdAsync(Guid id)
        {
            return await _tenantRepository.GetByIdAsync(id);
        }

        public async Task<Tenant?> GetBySubdomainAsync(string subdomain)
        {
            return await _tenantRepository.GetBySubdomainAsync(subdomain);
        }

        private string HashApiKey(string apiKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(apiKey);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
