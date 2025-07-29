using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CompliGenie.DTOs;

namespace CompliGenie.Tests.Integration
{
    public class PartnerRegistrationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PartnerRegistrationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task RegisterPartner_EndToEnd_Success()
        {
            // Arrange
            var dto = new PartnerRegistrationDto
            {
                CompanyName = "Integration Test Company",
                Email = "test@integrationcompany.com",
                Website = "https://integrationcompany.com",
                Industry = "Legal",
                Phone = "+1-555-0123",
                EstimatedMonthlyPolicies = 50
            };

            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/partners/register", content);

            // Assert
            response.EnsureSuccessStatusCode();
            
            var responseString = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseString);
            
            Assert.True(responseData.TryGetProperty("apiKey", out var apiKey));
            Assert.StartsWith("cg_live_", apiKey.GetString());
            
            Assert.True(responseData.TryGetProperty("subdomain", out var subdomain));
            Assert.Contains("integration-test-company", subdomain.GetString());
            
            Assert.True(responseData.TryGetProperty("dashboardUrl", out var dashboardUrl));
            Assert.True(responseData.TryGetProperty("stripeOnboardingUrl", out var stripeUrl));
        }
    }
}
