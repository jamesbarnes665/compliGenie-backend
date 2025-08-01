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
    public class PolicyGenerationTests
    {
        private readonly Mock<ILangChainService> _langChainServiceMock;
        private readonly Mock<ILogger<PolicyGenerator>> _loggerMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly PolicyGenerator _policyGenerator;

        public PolicyGenerationTests()
        {
            _langChainServiceMock = new Mock<ILangChainService>();
            _loggerMock = new Mock<ILogger<PolicyGenerator>>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            
            _currentTenantMock.Setup(x => x.Id).Returns(Guid.NewGuid());
            _currentTenantMock.Setup(x => x.Name).Returns("Test Partner");
            
            _policyGenerator = new PolicyGenerator(
                _langChainServiceMock.Object,
                _loggerMock.Object,
                _currentTenantMock.Object
            );
        }

        [Fact(DisplayName = "MinimumPageLength")]
        public async Task GeneratePolicy_ShouldMeetMinimumPageLength()
        {
            // Arrange
            var request = new PolicyRequest
            {
                Industry = "legal",
                CompanySize = 50,
                ClientName = "Test Law Firm",
                AITools = new[] { "ChatGPT", "Claude", "CaseText" }
            };

            var mockPolicy = new
            {
                Title = "AI Governance Policy for Test Law Firm",
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
                    Duration = TimeSpan.FromSeconds(8)
                });

            // Act
            var result = await _policyGenerator.GeneratePolicy(request);

            // Assert
            Assert.NotNull(result);
            Assert.InRange(result.PageCount, 8, 12);
            Assert.DoesNotContain("Lorem ipsum", result.Sections.SelectMany(s => s.Content));
        }

        [Fact]
        public async Task GeneratePolicy_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            var request = new PolicyRequest
            {
                Industry = "general",
                CompanySize = 100,
                ClientName = "Test Company"
            };

            var mockPolicy = new
            {
                Title = "AI Governance Policy",
                Sections = new[] { new { Title = "Section 1", Content = "Content" } }
            };

            _langChainServiceMock
                .Setup(x => x.Generate(It.IsAny<GenerationRequest>()))
                .ReturnsAsync(new LLMResponse
                {
                    Content = JsonSerializer.Serialize(mockPolicy),
                    Duration = TimeSpan.FromSeconds(9)
                });

            // Act
            var startTime = DateTime.UtcNow;
            await _policyGenerator.GeneratePolicy(request);
            var duration = DateTime.UtcNow - startTime;

            // Assert
            Assert.True(duration.TotalSeconds < 10);
        }
    }
}
