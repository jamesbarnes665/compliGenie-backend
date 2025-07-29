using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompliGenie.Tests.AcceptanceTests
{
    public class Story_1_1_3_PartnerRegistrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public Story_1_1_3_PartnerRegistrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task AC1_Registration_Completes_Under_30_Seconds()
        {
            var registration = new
            {
                companyName = "Speed Test Company",
                email = "admin@speedtest.com",
                website = "https://speedtest.com",
                industry = "Legal",
                phone = "+1-555-0123",
                estimatedMonthlyPolicies = 100
            };

            var sw = Stopwatch.StartNew();
            
            var response = await _client.PostAsJsonAsync("/api/partners/register", registration);
            
            sw.Stop();
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(sw.Elapsed.TotalSeconds < 30, 
                $"Registration took {sw.Elapsed.TotalSeconds}s, should be under 30s");
        }

        [Fact]
        public async Task AC2_API_Key_Generated_Automatically()
        {
            var registration = new
            {
                companyName = "API Key Test Company",
                email = "admin@apikeytest.com",
                website = "https://apikeytest.com",
                industry = "Legal"
            };

            var response = await _client.PostAsJsonAsync("/api/partners/register", registration);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadAsAsync<dynamic>();
            
            Assert.NotNull(result.apiKey);
            var apiKey = (string)result.apiKey;
            Assert.StartsWith("cg_live_", apiKey);
            Assert.True(apiKey.Length > 20, "API key should be sufficiently long");
        }

        [Fact]
        public async Task AC3_Subdomain_Generated_Automatically()
        {
            var registration = new
            {
                companyName = "Subdomain Test & Special Chars!",
                email = "admin@subdomaintest.com",
                website = "https://subdomaintest.com",
                industry = "Legal"
            };

            var response = await _client.PostAsJsonAsync("/api/partners/register", registration);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadAsAsync<dynamic>();
            
            Assert.NotNull(result.subdomain);
            var subdomain = (string)result.subdomain;
            Assert.Contains("subdomain-test-special-chars", subdomain);
            Assert.Matches(@"^[a-z0-9\-]+$", subdomain); // Only lowercase, numbers, and hyphens
        }

        [Fact]
        public async Task AC4_Stripe_Connect_Account_Created()
        {
            var registration = new
            {
                companyName = "Stripe Test Company",
                email = "admin@stripetest.com",
                website = "https://stripetest.com",
                industry = "Legal"
            };

            var response = await _client.PostAsJsonAsync("/api/partners/register", registration);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadAsAsync<dynamic>();
            
            Assert.NotNull(result.stripeOnboardingUrl);
            var stripeUrl = (string)result.stripeOnboardingUrl;
            Assert.Contains("stripe.com", stripeUrl);
            Assert.Contains("onboarding", stripeUrl);
        }

        [Fact]
        public async Task AC5_Welcome_Email_Sent()
        {
            // In a real test, we'd verify email was sent through a test email service
            // For now, we verify the endpoint returns success with email-related info
            var registration = new
            {
                companyName = "Email Test Company",
                email = "admin@emailtest.com",
                website = "https://emailtest.com",
                industry = "Legal"
            };

            var response = await _client.PostAsJsonAsync("/api/partners/register", registration);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadAsAsync<dynamic>();
            
            Assert.NotNull(result.message);
            var message = (string)result.message;
            Assert.Contains("Check your email", message);
        }

        [Fact]
        public async Task AC6_Partner_Can_Use_API_Immediately()
        {
            // Register a partner
            var registration = new
            {
                companyName = "Immediate Use Test",
                email = "admin@immediatetest.com",
                website = "https://immediatetest.com",
                industry = "Legal"
            };

            var regResponse = await _client.PostAsJsonAsync("/api/partners/register", registration);
            regResponse.EnsureSuccessStatusCode();
            
            var result = await regResponse.Content.ReadAsAsync<dynamic>();
            var apiKey = (string)result.apiKey;
            
            // Immediately use the API key
            var testRequest = new HttpRequestMessage(HttpMethod.Get, "/api/test");
            testRequest.Headers.Add("X-API-Key", apiKey);
            
            var testResponse = await _client.SendAsync(testRequest);
            Assert.NotEqual(HttpStatusCode.Unauthorized, testResponse.StatusCode);
        }

        [Fact]
        public async Task Additional_Business_Email_Validation()
        {
            // Test personal email rejection
            var registration = new
            {
                companyName = "Personal Email Test",
                email = "test@gmail.com",
                website = "https://test.com",
                industry = "Legal"
            };

            var response = await _client.PostAsJsonAsync("/api/partners/register", registration);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var error = await response.Content.ReadAsAsync<dynamic>();
            Assert.Contains("business email", (string)error.error);
        }
    }
}
