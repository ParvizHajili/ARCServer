using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Areas.Dashboard.Controllers
{
    /// <summary>
    /// Dashboard API surface — route prefix: api/dashboard/...
    /// </summary>
    [Area("Dashboard")]
    [ApiController]
    [Route("api/dashboard/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { scope = "dashboard", status = "ok" });
        }
    }
}
