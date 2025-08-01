using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompliGenie.Data;

namespace CompliGenie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DbTestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DbTestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetDatabaseStatus()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var tenantCount = await _context.Tenants.CountAsync();
                
                return Ok(new 
                { 
                    canConnect = canConnect,
                    tenantCount = tenantCount,
                    hasTenants = tenantCount > 0,
                    connectionString = _context.Database.GetConnectionString()
                });
            }
            catch (Exception ex)
            {
                return Ok(new 
                { 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
