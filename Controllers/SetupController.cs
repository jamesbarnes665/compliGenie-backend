using Microsoft.AspNetCore.Mvc;
using CompliGenie.Data;
using CompliGenie.Models;
using Microsoft.EntityFrameworkCore;

namespace CompliGenie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SetupController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("test-tenant")]
        public async Task<IActionResult> CreateTestTenant()
        {
            // Check if test tenant already exists
            var existing = await _context.Tenants
                .FirstOrDefaultAsync(t => t.ApiKeyHash == "test-api-key-123");
                
            if (existing != null)
            {
                return Ok(new { 
                    message = "Test tenant already exists",
                    tenantId = existing.Id,
                    apiKey = "test-api-key-123"
                });
            }

            // Create new test tenant
            var testTenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                Subdomain = "test",
                ApiKeyHash = "test-api-key-123",
                StripeAccountId = null,
                Settings = "{}",
                CreatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(testTenant);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Test tenant created successfully",
                tenantId = testTenant.Id,
                apiKey = "test-api-key-123"
            });
        }

        [HttpGet("tenants")]
        public async Task<IActionResult> GetAllTenants()
        {
            var tenants = await _context.Tenants.ToListAsync();
            return Ok(tenants);
        }
    }
}
