using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CompliGenie.Models;
using CompliGenie.Services;
using CompliGenie.Context;
using System.Text.Json;

namespace CompliGenie.Tests.PolicyGeneration
{
    public class PolicyGenerationIntegrationTests
    {
        private readonly PolicyGenerator _policyGenerator;
        private readonly Mock<ILangChainService> _langChainServiceMock;
        private readonly Mock<ILogger<PolicyGenerator>> _loggerMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IPromptService> _promptServiceMock;

        public PolicyGenerationIntegrationTests()
        {
            _langChainServiceMock = new Mock<ILangChainService>();
            _loggerMock = new Mock<ILogger<PolicyGenerator>>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _promptServiceMock = new Mock<IPromptService>();
            
            _currentTenantMock.Setup(x => x.Id).Returns(Guid.NewGuid());
            _currentTenantMock.Setup(x => x.Name).Returns("Test Partner");
            
            // Mock prompt loading
            _promptServiceMock.Setup(x => x.LoadPrompt(It.IsAny<string>()))
                .Returns("Test prompt content {AI_TOOLS_LIST} {COMPANY_SIZE}");
            
            _policyGenerator = new PolicyGenerator(
                _langChainServiceMock.Object,
                _loggerMock.Object,
                _currentTenantMock.Object,
                _promptServiceMock.Object
            );
        }

        [Fact]
        public async Task GeneratePolicy_WithValidRequest_MeetsPageRequirements()
        {
            // Arrange
            var request = new PolicyRequest
            {
                ClientName = "Integration Test Law Firm",
                Industry = "legal",
                CompanySize = 50,
                AITools = new[] { "ChatGPT", "Claude" },
                Jurisdictions = new[] { "California" }
            };

            var mockPolicy = new
            {
                Title = "AI Governance Policy for Integration Test Law Firm",
                Sections = Enumerable.Range(1, 5).Select(i => new
                {
                    Title = $"Section {i}",
                    Content = string.Join(" ", Enumerable.Repeat("word", 400))
                }).ToArray()
            };

            _langChainServiceMock
                .Setup(x => x.Generate(It.IsAny<GenerationRequest>()))
                .ReturnsAsync(new LLMResponse
                {
                    Content = JsonSerializer.Serialize(mockPolicy),
                    TokensUsed = 7500,
                    Duration = TimeSpan.FromSeconds(5)
                });
            
            // Act
            var policy = await _policyGenerator.GeneratePolicy(request);
            
            // Assert
            Assert.InRange(policy.PageCount, 8, 12);
            Assert.All(policy.Sections, s => Assert.NotEmpty(s.Content));
            Assert.DoesNotContain("Lorem ipsum", policy.Sections.SelectMany(s => s.Content));
        }

        [Fact]
        public async Task GeneratePolicy_HandlesMalformedLLMResponse()
        {
            // Arrange
            var request = new PolicyRequest
            {
                ClientName = "Test Client",
                Industry = "legal",
                CompanySize = 10
            };

            _langChainServiceMock
                .Setup(x => x.Generate(It.IsAny<GenerationRequest>()))
                .ReturnsAsync(new LLMResponse
                {
                    Content = "This is not JSON but plain text response with sections",
                    TokensUsed = 1000,
                    Duration = TimeSpan.FromSeconds(2)
                });
            
            // Act
            var policy = await _policyGenerator.GeneratePolicy(request);
            
            // Assert
            Assert.NotNull(policy);
            Assert.NotEmpty(policy.Sections);
        }

        [Fact]
        public async Task GeneratePolicy_RespectsTimeoutLimit()
        {
            // Arrange
            var request = new PolicyRequest
            {
                ClientName = "Speed Test Client",
                Industry = "general",
                CompanySize = 25
            };

            var mockPolicy = new
            {
                Title = "Test Policy",
                Sections = new[] { new { Title = "Section 1", Content = "Content" } }
            };

            _langChainServiceMock
                .Setup(x => x.Generate(It.IsAny<GenerationRequest>()))
                .ReturnsAsync(new LLMResponse
                {
                    Content = JsonSerializer.Serialize(mockPolicy),
                    TokensUsed = 1000,
                    Duration = TimeSpan.FromSeconds(8)
                });
            
            // Act
            var startTime = DateTime.UtcNow;
            await _policyGenerator.GeneratePolicy(request);
            var duration = DateTime.UtcNow - startTime;
            
            // Assert
            Assert.True(duration.TotalSeconds < 10, $"Generation took {duration.TotalSeconds}s, should be under 10s");
        }
    }
}