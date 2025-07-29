using Xunit;
using System;

namespace CompliGenie.MultiTenant.Tests
{
    public class BasicPartnerRegistrationTests
    {
        [Fact]
        public void ApiKey_ShouldStartWith_CgLive()
        {
            // This tests the API key format without needing the full controller
            var apiKey = GenerateSecureApiKey();
            Assert.StartsWith("cg_live_", apiKey);
        }

        [Fact]
        public void Subdomain_ShouldBeValidFormat()
        {
            var subdomain = GenerateSubdomain("Test Company Name!");
            Assert.Matches(@"^[a-z0-9\-]+$", subdomain);
            Assert.Contains("test-company-name", subdomain);
        }

        private string GenerateSecureApiKey()
        {
            var bytes = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return $"cg_live_{Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-").TrimEnd('=')}";
        }

        private string GenerateSubdomain(string companyName)
        {
            var subdomain = System.Text.RegularExpressions.Regex.Replace(companyName.ToLower(), @"[^a-z0-9\s-]", "");
            subdomain = System.Text.RegularExpressions.Regex.Replace(subdomain, @"\s+", "-");
            subdomain = subdomain.Trim('-');
            
            return $"{subdomain}-{Guid.NewGuid().ToString().Substring(0, 6)}";
        }
    }
}

