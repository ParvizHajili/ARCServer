using ARCServer.Business.Dtos.Catalog;
using ARCServer.Business.Services.Catalog;
using ARCServer.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Controllers
{
    [ApiController]
    [Route("api/filters")]
    public class PublicFiltersController : ApiControllerBase
    {
        private readonly ICatalogFilterService _service;

        public PublicFiltersController(ICatalogFilterService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CatalogFilterOptionsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(
            [FromQuery] string? lang,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetOptionsAsync(lang, cancellationToken);
            return FromResult(result);
        }
    }
}
