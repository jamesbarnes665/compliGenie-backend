using Microsoft.AspNetCore.Mvc;

namespace CompliGenie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var tenantId = HttpContext.Items["TenantId"];
            return Ok(new { 
                message = "API is working", 
                tenantId = tenantId,
                authenticated = tenantId != null 
            });
        }
    }
}
