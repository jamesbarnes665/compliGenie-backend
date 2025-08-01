using System.Threading.Tasks;
using CompliGenie.Models;
using CompliGenie.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompliGenie.Services
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendWelcomeEmailAsync(string email, Tenant tenant)
        {
            _logger.LogInformation("Sending welcome email to {Email} for tenant {Tenant}", 
                email, tenant.Name);
            return Task.CompletedTask;
        }

        public Task SendApiCredentialsAsync(string email, string apiKey, string subdomain)
        {
            _logger.LogInformation("Sending API credentials to {Email}", email);
            return Task.CompletedTask;
        }

        public Task SendOnboardingInstructionsAsync(string email, string onboardingUrl)
        {
            _logger.LogInformation("Sending onboarding instructions to {Email}", email);
            return Task.CompletedTask;
        }
    }
}
