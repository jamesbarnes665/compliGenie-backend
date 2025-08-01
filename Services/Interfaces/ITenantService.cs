using System;
using System.Threading.Tasks;
using CompliGenie.Models;

namespace CompliGenie.Services.Interfaces
{
    public interface ITenantService
    {
        Task<Tenant?> GetByApiKeyAsync(string apiKey);
        Task<Tenant?> GetByIdAsync(Guid id);
        Task<Tenant?> GetBySubdomainAsync(string subdomain);
    }
}
