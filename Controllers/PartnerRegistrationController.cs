using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CompliGenie.DTOs;
using CompliGenie.Models;
using CompliGenie.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Linq;

namespace CompliGenie.Controllers
{
    [ApiController]
    [Route("api/partners")]
    public class PartnerRegistrationController : ControllerBase
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IStripeService _stripeService;
        private readonly IEmailService _emailService;
        private readonly ILogger<PartnerRegistrationController> _logger;

        public PartnerRegistrationController(
            ITenantRepository tenantRepository,
            IStripeService stripeService,
            IEmailService emailService,
            ILogger<PartnerRegistrationController> logger)
        {
            _tenantRepository = tenantRepository;
            _stripeService = stripeService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterPartner([FromBody] PartnerRegistrationDto dto)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                // Validate business information
                if (!IsValidBusinessEmail(dto.Email))
                {
                    return BadRequest(new { error = "Please use a business email address" });
                }
                
                // Check if company already exists
                if (await _tenantRepository.ExistsAsync(dto.CompanyName))
                {
                    return Conflict(new { error = "Company name already registered" });
                }
                
                // Create Stripe Connect account
                var stripeAccountId = await _stripeService.CreateConnectAccountAsync(dto);
                
                // Generate secure credentials
                var apiKey = GenerateSecureApiKey();
                var subdomain = GenerateSubdomain(dto.CompanyName);
                
                // Create tenant
                var tenant = new Tenant
                {
                    Name = dto.CompanyName,
                    Subdomain = subdomain,
                    ApiKeyHash = HashApiKey(apiKey),
                    StripeAccountId = stripeAccountId,
                    Settings = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        industry = dto.Industry,
                        website = dto.Website,
                        phone = dto.Phone,
                        estimatedMonthlyPolicies = dto.EstimatedMonthlyPolicies,
                        registrationDate = DateTime.UtcNow
                    })
                };
                
                await _tenantRepository.CreateAsync(tenant);
                
                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(dto.Email, tenant);
                
                // Log performance
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("Partner registration completed in {Duration}ms for {Company}", 
                    duration.TotalMilliseconds, dto.CompanyName);
                
                return Ok(new
                {
                    apiKey = apiKey,
                    subdomain = tenant.Subdomain,
                    dashboardUrl = $"https://{tenant.Subdomain}.compligenie.com",
                    stripeOnboardingUrl = await _stripeService.GetOnboardingLinkAsync(stripeAccountId),
                    message = "Registration successful! Check your email for onboarding instructions."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering partner {Company}", dto.CompanyName);
                return StatusCode(500, new { error = "Registration failed. Please try again." });
            }
        }

        private bool IsValidBusinessEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
                
            var freeEmailDomains = new[] 
            { 
                "gmail.com", "yahoo.com", "hotmail.com", "outlook.com", 
                "aol.com", "icloud.com", "mail.com", "protonmail.com" 
            };
            
            var domain = email.Split('@').LastOrDefault()?.ToLower();
            return !string.IsNullOrEmpty(domain) && !freeEmailDomains.Contains(domain);
        }

        private string GenerateSubdomain(string companyName)
        {
            var subdomain = Regex.Replace(companyName.ToLower(), @"[^a-z0-9\s-]", "");
            subdomain = Regex.Replace(subdomain, @"\s+", "-");
            subdomain = subdomain.Trim('-');
            
            return $"{subdomain}-{Guid.NewGuid().ToString().Substring(0, 6)}";
        }

        private string GenerateSecureApiKey()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return $"cg_live_{Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-").TrimEnd('=')}";
        }

        private string HashApiKey(string apiKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(apiKey);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
