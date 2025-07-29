using System.Threading.Tasks;
using CompliGenie.Models;

namespace CompliGenie.Services
{
    public interface ITenantService
    {
        Task<Tenant?> GetByApiKey(string apiKey);
    }
}
