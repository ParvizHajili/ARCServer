using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Controllers
{
    /// <summary>
    /// UI (public storefront) API surface — route prefix: api/...
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { scope = "ui", status = "ok" });
        }
    }
}
