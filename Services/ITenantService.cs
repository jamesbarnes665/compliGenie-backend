using System;
using System.Threading.Tasks;
using CompliGenie.Models;

namespace CompliGenie.Services
{
    public interface ITenantService
    {
        Task<Tenant?> GetByApiKeyAsync(string apiKey);
    }
}
