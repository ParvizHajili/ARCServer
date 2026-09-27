using ARCServer.Business.Dtos.Dashboard;
using ARCServer.Business.Services.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Areas.Dashboard.Controllers
{
    [Route("api/dashboard/overview")]
    public class OverviewController : DashboardControllerBase
    {
        private readonly IDashboardService _service;

        public OverviewController(IDashboardService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(DashboardOverviewDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _service.GetOverviewAsync(cancellationToken);
            return FromResult(result);
        }
    }
}
