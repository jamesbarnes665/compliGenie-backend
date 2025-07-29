using Microsoft.AspNetCore.Mvc;
using CompliGenie.Services;
using CompliGenie.Models;
using CompliGenie.Context;
using System;
using System.Threading.Tasks;

namespace CompliGenie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoAuthTestController : ControllerBase
    {
        private readonly IPolicyGenerator _policyGenerator;
        private readonly ICurrentTenant _currentTenant;

        public NoAuthTestController(IPolicyGenerator policyGenerator, ICurrentTenant currentTenant)
        {
            _policyGenerator = policyGenerator;
            _currentTenant = currentTenant;
        }

        [HttpPost("test-policy")]
        public async Task<IActionResult> TestPolicy([FromBody] PolicyRequest request)
        {
            // Bypass auth for testing
            _currentTenant.Id = Guid.NewGuid();
            _currentTenant.Name = "Test";
            
            try
            {
                var policy = await _policyGenerator.GeneratePolicy(request);
                return Ok(new { 
                    success = true, 
                    pageCount = policy.PageCount,
                    sections = policy.Sections.Count
                });
            }
            catch (Exception ex)
            {
                return Ok(new { 
                    success = false, 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}
