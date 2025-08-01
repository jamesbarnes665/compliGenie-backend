using Microsoft.AspNetCore.Mvc;
using CompliGenie.Data;
using CompliGenie.Models;
using Microsoft.EntityFrameworkCore;

namespace CompliGenie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PoliciesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PoliciesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPolicies()
        {
            var tenantId = HttpContext.Items["TenantId"];
            if (tenantId == null)
            {
                return Unauthorized("No tenant context");
            }

            // This will only return policies for the current tenant
            var policies = await _context.Policies
                .Where(p => p.TenantId == (Guid)tenantId)
                .ToListAsync();
            
            return Ok(new { tenantId, policies });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyDto dto)
        {
            var tenantId = (Guid)HttpContext.Items["TenantId"]!;
            
            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClientName = dto.ClientName,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();

            return Ok(policy);
        }
    }

    public class CreatePolicyDto
    {
        public string ClientName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
