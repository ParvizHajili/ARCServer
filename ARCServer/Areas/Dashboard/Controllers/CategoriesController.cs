using ARCServer.Authorization;
using ARCServer.Business.Dtos.Categories;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Services.Categories;
using ARCServer.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Areas.Dashboard.Controllers
{
    [Route("api/dashboard/categories")]
    public class CategoriesController : DashboardControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.Categories.List)]
        [ProducesResponseType(typeof(PaginationResponseDto<CategoryDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _categoryService.GetPagedAsync(request, cancellationToken);
            return FromResult(result);
        }

        [HttpGet("{id:int}")]
        [RequirePermission(PermissionCodes.Categories.View)]
        [ProducesResponseType(typeof(CategoryDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _categoryService.GetByIdAsync(id, cancellationToken);
            return FromResult(result);
        }

        [HttpPost]
        [RequirePermission(PermissionCodes.Categories.Create)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CategoryDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromForm] CategoryFormRequest request,
            CancellationToken cancellationToken)
        {
            var mapped = CategoryFormMapper.ToCreateDto(request);
            if (!mapped.Succeeded)
            {
                return FromFailure(mapped);
            }

            var result = await _categoryService.CreateAsync(mapped.Data!, creatorId: null, cancellationToken);
            return FromCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
        }

        [HttpPut("{id:int}")]
        [RequirePermission(PermissionCodes.Categories.Update)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CategoryDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] CategoryFormRequest request,
            CancellationToken cancellationToken)
        {
            var mapped = CategoryFormMapper.ToUpdateDto(request);
            if (!mapped.Succeeded)
            {
                return FromFailure(mapped);
            }

            var result = await _categoryService.UpdateAsync(id, mapped.Data!, updaterId: null, cancellationToken);
            return FromResult(result);
        }

        [HttpDelete("{id:int}")]
        [RequirePermission(PermissionCodes.Categories.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _categoryService.DeleteAsync(id, deletorId: null, cancellationToken);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return FromFailure(result);
        }
    }
}
