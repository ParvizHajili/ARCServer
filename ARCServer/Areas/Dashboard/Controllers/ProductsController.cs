using ARCServer.Authorization;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.Products;
using ARCServer.Business.Services.Products;
using ARCServer.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Areas.Dashboard.Controllers
{
    [Route("api/dashboard/products")]
    public class ProductsController : DashboardControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        [HttpGet("home")]
        [RequirePermission(PermissionCodes.Products.List)]
        [ProducesResponseType(typeof(List<HomeProductDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHomeProducts(CancellationToken cancellationToken)
        {
            var result = await _service.GetHomeProductsAsync(cancellationToken);
            return FromResult(result);
        }

        [HttpGet("home/options")]
        [RequirePermission(PermissionCodes.Products.List)]
        [ProducesResponseType(typeof(List<HomeProductOptionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchHomeOptions(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            var result = await _service.SearchHomeProductOptionsAsync(search, cancellationToken);
            return FromResult(result);
        }

        [HttpPut("home")]
        [RequirePermission(PermissionCodes.Products.Update)]
        [ProducesResponseType(typeof(List<HomeProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetHomeProducts(
            [FromBody] SetHomeProductsDto request,
            CancellationToken cancellationToken)
        {
            var result = await _service.SetHomeProductsAsync(request, cancellationToken);
            return FromResult(result);
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.Products.List)]
        [ProducesResponseType(typeof(PaginationResponseDto<ProductListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return FromResult(result);
        }

        [HttpGet("{id:int}")]
        [RequirePermission(PermissionCodes.Products.View)]
        [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return FromResult(result);
        }

        [HttpPost]
        [RequirePermission(PermissionCodes.Products.Create)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromForm] ProductFormRequest request,
            CancellationToken cancellationToken)
        {
            var mapped = ProductFormMapper.ToCreateDto(request);
            if (!mapped.Succeeded)
            {
                return FromFailure(mapped);
            }

            var result = await _service.CreateAsync(mapped.Data!, creatorId: null, cancellationToken);
            return FromCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
        }

        [HttpPut("{id:int}")]
        [RequirePermission(PermissionCodes.Products.Update)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] ProductFormRequest request,
            CancellationToken cancellationToken)
        {
            var mapped = ProductFormMapper.ToUpdateDto(request);
            if (!mapped.Succeeded)
            {
                return FromFailure(mapped);
            }

            var result = await _service.UpdateAsync(id, mapped.Data!, updaterId: null, cancellationToken);
            return FromResult(result);
        }

        [HttpDelete("{id:int}")]
        [RequirePermission(PermissionCodes.Products.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteAsync(id, deletorId: null, cancellationToken);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return FromFailure(result);
        }
    }
}
