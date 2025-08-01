using System;
using System.Threading.Tasks;
using CompliGenie.DTOs;
using CompliGenie.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompliGenie.Services
{
    public class MockStripeService : IStripeService
    {
        private readonly ILogger<MockStripeService> _logger;

        public MockStripeService(ILogger<MockStripeService> logger)
        {
            _logger = logger;
        }

        public Task<string> CreateConnectAccountAsync(PartnerRegistrationDto dto)
        {
            _logger.LogInformation("Creating mock Stripe Connect account for {Company}", dto.CompanyName);
            return Task.FromResult($"acct_mock_{Guid.NewGuid().ToString().Substring(0, 16)}");
        }

        public Task<bool> VerifyAccountAsync(string accountId)
        {
            return Task.FromResult(true);
        }

        public Task<string> GetOnboardingLinkAsync(string accountId)
        {
            return Task.FromResult($"https://connect.stripe.com/mock/onboarding/{accountId}");
        }
    }
}
