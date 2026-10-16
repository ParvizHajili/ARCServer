using ARCServer.Business.Dtos.Products;
using ARCServer.Business.Services.Dashboard;
using ARCServer.Business.Services.Products;
using ARCServer.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class PublicProductsController : ApiControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IProductService _productService;

        public PublicProductsController(
            IDashboardService dashboardService,
            IProductService productService)
        {
            _dashboardService = dashboardService;
            _productService = productService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCatalog(
            [FromQuery] ProductCatalogQueryDto request,
            CancellationToken cancellationToken)
        {
            var result = await _productService.GetCatalogPagedAsync(request, cancellationToken);
            return FromResult(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            int id,
            [FromQuery] string? lang,
            CancellationToken cancellationToken)
        {
            var result = await _productService.GetPublicDetailAsync(id, lang, cancellationToken);
            if (result.Succeeded)
            {
                Response.Headers.CacheControl = "public, max-age=60";
            }

            return FromResult(result);
        }

        [HttpPost("views")]
        public async Task<IActionResult> RecordView(
            [FromQuery] string code,
            CancellationToken cancellationToken)
        {
            var result = await _dashboardService.RecordViewByCodeAsync(code, cancellationToken);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return FromFailure(result);
        }
    }
}
