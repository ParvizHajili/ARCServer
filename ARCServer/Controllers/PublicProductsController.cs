using ARCServer.Business.Services.Dashboard;
using ARCServer.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/products")]
    public class PublicProductsController : ApiControllerBase
    {
        private readonly IDashboardService _service;

        public PublicProductsController(IDashboardService service)
        {
            _service = service;
        }

        [HttpPost("views")]
        public async Task<IActionResult> RecordView(
            [FromQuery] string code,
            CancellationToken cancellationToken)
        {
            var result = await _service.RecordViewByCodeAsync(code, cancellationToken);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return FromFailure(result);
        }
    }
}
