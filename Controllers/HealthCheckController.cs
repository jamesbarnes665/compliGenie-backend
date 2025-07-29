using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompliGenie.Data;

namespace CompliGenie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthCheckController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // This endpoint is excluded from auth in Program.cs
                var canConnect = await _context.Database.CanConnectAsync();
                var tableCount = 0;
                
                try 
                {
                    // Try to count tenants without auth
                    tableCount = await _context.Tenants.CountAsync();
                }
                catch (Exception)
                {
                    // Table might not exist
                }
                
                return Ok(new 
                { 
                    status = "healthy",
                    database = canConnect ? "connected" : "disconnected",
                    tenantCount = tableCount,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Ok(new 
                { 
                    status = "error",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}
