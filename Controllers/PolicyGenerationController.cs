using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CompliGenie.Models;
using CompliGenie.Services;
using CompliGenie.Context;

namespace CompliGenie.Controllers
{
    [ApiController]
    [Route("api/policies")]
    public class PolicyGenerationController : ControllerBase
    {
        private readonly IPolicyGenerator _policyGenerator;
        private readonly ILogger<PolicyGenerationController> _logger;
        private readonly ICurrentTenant _currentTenant;

        public PolicyGenerationController(
            IPolicyGenerator policyGenerator,
            ILogger<PolicyGenerationController> logger,
            ICurrentTenant currentTenant)
        {
            _policyGenerator = policyGenerator;
            _logger = logger;
            _currentTenant = currentTenant;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePolicy([FromBody] PolicyRequest request)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                if (!_currentTenant.IsSet)
                {
                    return Unauthorized("No tenant context");
                }

                _logger.LogInformation("Generating policy for tenant {TenantId}, client {ClientName}", 
                    _currentTenant.Id, request.ClientName);

                var policy = await _policyGenerator.GeneratePolicy(request);
                var duration = DateTime.UtcNow - startTime;

                return Ok(new
                {
                    policyId = policy.Id,
                    title = policy.Title,
                    pageCount = policy.PageCount,
                    sections = policy.Sections.Count,
                    generatedAt = policy.GeneratedAt,
                    duration = duration.TotalSeconds,
                    message = "Policy generated successfully"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid policy request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating policy");
                return StatusCode(500, new { error = "Failed to generate policy" });
            }
        }
    }
}
