using System.Threading.Tasks;
using CompliGenie.Models;

namespace CompliGenie.Services
{
    public interface IPdfGenerationService
    {
        Task<byte[]> GeneratePdf(PolicyDocument policy, TenantBranding branding);
    }
}