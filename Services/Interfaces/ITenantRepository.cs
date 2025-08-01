using System;
using System.Threading.Tasks;
using CompliGenie.Models;

namespace CompliGenie.Services.Interfaces
{
    public interface ITenantRepository
    {
        Task<Tenant> CreateAsync(Tenant tenant);
        Task<Tenant?> GetByIdAsync(Guid id);
        Task<Tenant?> GetBySubdomainAsync(string subdomain);
        Task<bool> ExistsAsync(string companyName);
        
        Task<List<Tenant>> GetAllAsync();
    }
}

