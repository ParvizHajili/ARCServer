using ARCServer.Business.Dtos.Categories;
using ARCServer.Business.Services.Categories;
using ARCServer.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/categories")]
    public class PublicCategoriesController : ApiControllerBase
    {
        private readonly ICategoryService _service;

        public PublicCategoriesController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<CategoryDetailDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await _service.GetCatalogAsync(cancellationToken);
            return FromResult(result);
        }
    }
}
