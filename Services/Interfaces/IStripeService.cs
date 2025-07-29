using System.Threading.Tasks;
using CompliGenie.DTOs;

namespace CompliGenie.Services.Interfaces
{
    public interface IStripeService
    {
        Task<string> CreateConnectAccountAsync(PartnerRegistrationDto dto);
        Task<bool> VerifyAccountAsync(string accountId);
        Task<string> GetOnboardingLinkAsync(string accountId);
    }
}
