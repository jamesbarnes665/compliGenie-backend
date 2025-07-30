using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using CompliGenie.DTOs;
using System.IO;

namespace CompliGenie.Tests.E2E
{
    public class PdfGenerationE2ETests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PdfGenerationE2ETests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GeneratePdf_WithValidPolicy_ReturnsPdfFile()
        {
            // Arrange - Register partner and get API key
            var registration = new PartnerRegistrationDto
            { 
                CompanyName = "PDF Test Company",
                Email = "pdf@testcompany.com",
                Website = "https://pdftestcompany.com",
                Industry = "Legal"
            };
            
            var regResponse = await _client.PostAsJsonAsync("/api/partners/register", registration);
            regResponse.EnsureSuccessStatusCode();
            
            var regResult = await regResponse.Content.ReadFromJsonAsync<dynamic>();
            var apiKey = regResult.GetProperty("apiKey").GetString();
            
            // Add API key to client
            _client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            
            // Act - Generate PDF (using a test policy ID for now)
            var policyId = Guid.NewGuid();
            var pdfResponse = await _client.GetAsync($"/api/policies/{policyId}/pdf");
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, pdfResponse.StatusCode);
            Assert.Equal("application/pdf", pdfResponse.Content.Headers.ContentType.MediaType);
            
            var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();
            Assert.NotEmpty(pdfBytes);
            
            // Verify it's a valid PDF (PDFs start with %PDF)
            var pdfHeader = System.Text.Encoding.UTF8.GetString(pdfBytes.Take(4).ToArray());
            Assert.Equal("%PDF", pdfHeader);
        }
    }
}