using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using CompliGenie.DTOs;
using CompliGenie.Models;

namespace CompliGenie.Tests.E2E
{
    public class PolicyGenerationE2ETests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PolicyGenerationE2ETests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task FullPolicyGenerationFlow_FromRegistrationToOutput()
        {
            // Step 1: Register partner
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var registration = new PartnerRegistrationDto
            { 
                CompanyName = $"E2E Policy Test {uniqueId}",
                Email = $"test@policytest{uniqueId}.com",
                Website = $"https://policytest{uniqueId}.com",
                Industry = "Legal"
            };
            
            var regResponse = await _client.PostAsJsonAsync("/api/partners/register", registration);
            regResponse.EnsureSuccessStatusCode();
            
            var regResult = await regResponse.Content.ReadFromJsonAsync<dynamic>();
            var apiKey = regResult.GetProperty("apiKey").GetString();
            
            // Step 2: Generate policy with API key
            _client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            
            var policyRequest = new PolicyRequest
            {
                ClientName = "Test Client LLC",
                Industry = "legal",
                CompanySize = 50,
                AITools = new[] { "ChatGPT", "Claude" },
                Jurisdictions = new[] { "California", "New York" }
            };
            
            var policyResponse = await _client.PostAsJsonAsync("/api/policies/generate", policyRequest);
            
            // Step 3: Verify response
            Assert.Equal(HttpStatusCode.OK, policyResponse.StatusCode);
            
            var policyResult = await policyResponse.Content.ReadFromJsonAsync<dynamic>();
            var pageCount = policyResult.GetProperty("pageCount").GetInt32();
            
            Assert.InRange(pageCount, 8, 12);
            Assert.True(policyResult.TryGetProperty("policyId", out _));
            Assert.True(policyResult.TryGetProperty("sections", out _));
        }

        [Fact]
        public async Task PolicyGeneration_WithoutApiKey_Returns401()
        {
            var policyRequest = new PolicyRequest
            {
                ClientName = "Unauthorized Test",
                Industry = "legal",
                CompanySize = 10
            };
            
            var response = await _client.PostAsJsonAsync("/api/policies/generate", policyRequest);
            
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PolicyGeneration_WithInvalidIndustry_ReturnsBadRequest()
        {
            // Setup with valid API key first
            var registration = new PartnerRegistrationDto
            { 
                CompanyName = "Invalid Industry Test",
                Email = "test@invalidindustry.com",
                Website = "https://invalidindustry.com",
                Industry = "Legal"
            };
            
            var regResponse = await _client.PostAsJsonAsync("/api/partners/register", registration);
            var apiKey = (await regResponse.Content.ReadFromJsonAsync<dynamic>())
                .GetProperty("apiKey").GetString();
            
            _client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            
            var policyRequest = new PolicyRequest
            {
                ClientName = "Test Client",
                Industry = "invalid_industry",
                CompanySize = 50
            };
            
            var response = await _client.PostAsJsonAsync("/api/policies/generate", policyRequest);
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}