using System.Threading.Tasks;
using CompliGenie.Models;

namespace CompliGenie.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string email, Tenant tenant);
        Task SendApiCredentialsAsync(string email, string apiKey, string subdomain);
        Task SendOnboardingInstructionsAsync(string email, string onboardingUrl);
    }
}
