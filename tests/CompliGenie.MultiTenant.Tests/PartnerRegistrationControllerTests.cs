using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using CompliGenie.Controllers;
using CompliGenie.DTOs;
using CompliGenie.Models;
using CompliGenie.Services.Interfaces;

namespace CompliGenie.Tests.Controllers
{
    public class PartnerRegistrationControllerTests
    {
        private readonly Mock<ITenantRepository> _tenantRepositoryMock;
        private readonly Mock<IStripeService> _stripeServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<PartnerRegistrationController>> _loggerMock;
        private readonly PartnerRegistrationController _controller;

        public PartnerRegistrationControllerTests()
        {
            _tenantRepositoryMock = new Mock<ITenantRepository>();
            _stripeServiceMock = new Mock<IStripeService>();
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<PartnerRegistrationController>>();
            
            _controller = new PartnerRegistrationController(
                _tenantRepositoryMock.Object,
                _stripeServiceMock.Object,
                _emailServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task RegisterPartner_WithPersonalEmail_ReturnsBadRequest()
        {
            // Arrange
            var dto = new PartnerRegistrationDto
            {
                CompanyName = "Test Company",
                Email = "test@gmail.com", // Personal email
                Website = "https://test.com",
                Industry = "Legal"
            };

            // Act
            var result = await _controller.RegisterPartner(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            dynamic value = badRequestResult.Value;
            Assert.Equal("Please use a business email address", value.error);
        }

        [Fact]
        public async Task RegisterPartner_WithExistingCompany_ReturnsConflict()
        {
            // Arrange
            var dto = new PartnerRegistrationDto
            {
                CompanyName = "Existing Company",
                Email = "test@company.com",
                Website = "https://company.com",
                Industry = "Legal"
            };

            _tenantRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RegisterPartner(dto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            dynamic value = conflictResult.Value;
            Assert.Equal("Company name already registered", value.error);
        }

        [Fact]
        public async Task RegisterPartner_WithValidData_ReturnsOkWithApiKey()
        {
            // Arrange
            var dto = new PartnerRegistrationDto
            {
                CompanyName = "New Company",
                Email = "admin@newcompany.com",
                Website = "https://newcompany.com",
                Industry = "Legal",
                EstimatedMonthlyPolicies = 100
            };

            _tenantRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            
            _stripeServiceMock.Setup(x => x.CreateConnectAccountAsync(It.IsAny<PartnerRegistrationDto>()))
                .ReturnsAsync("acct_test123");
            
            _stripeServiceMock.Setup(x => x.GetOnboardingLinkAsync(It.IsAny<string>()))
                .ReturnsAsync("https://connect.stripe.com/onboarding/test");

            _tenantRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Tenant>()))
                .ReturnsAsync((Tenant t) => t);

            // Act
            var result = await _controller.RegisterPartner(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic value = okResult.Value;
            
            Assert.NotNull(value.apiKey);
            Assert.StartsWith("cg_live_", (string)value.apiKey);
            Assert.NotNull(value.subdomain);
            Assert.Contains("new-company", (string)value.subdomain);
            Assert.NotNull(value.dashboardUrl);
            Assert.NotNull(value.stripeOnboardingUrl);
            
            // Verify all services were called
            _stripeServiceMock.Verify(x => x.CreateConnectAccountAsync(It.IsAny<PartnerRegistrationDto>()), Times.Once);
            _emailServiceMock.Verify(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<Tenant>()), Times.Once);
            _tenantRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Tenant>()), Times.Once);
        }

        [Fact]
        public async Task RegisterPartner_CompletesWithinTimeLimit()
        {
            // Arrange
            var dto = new PartnerRegistrationDto
            {
                CompanyName = "Performance Test Company",
                Email = "test@performancecompany.com",
                Website = "https://performancecompany.com",
                Industry = "Legal"
            };

            _tenantRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _stripeServiceMock.Setup(x => x.CreateConnectAccountAsync(It.IsAny<PartnerRegistrationDto>()))
                .ReturnsAsync("acct_test123");
            _stripeServiceMock.Setup(x => x.GetOnboardingLinkAsync(It.IsAny<string>()))
                .ReturnsAsync("https://connect.stripe.com/onboarding/test");

            // Act
            var startTime = DateTime.UtcNow;
            var result = await _controller.RegisterPartner(dto);
            var duration = DateTime.UtcNow - startTime;

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.True(duration.TotalSeconds < 30, $"Registration took {duration.TotalSeconds} seconds, should be under 30");
        }
    }
}
