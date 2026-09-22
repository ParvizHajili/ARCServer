using ARCServer.Authorization;
using ARCServer.Business.Dtos.Colors;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Services.Colors;
using ARCServer.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Areas.Dashboard.Controllers
{
    [Route("api/dashboard/colors")]
    public class ColorsController : DashboardControllerBase
    {
        private readonly IColorService _service;

        public ColorsController(IColorService service)
        {
            _service = service;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.Colors.List)]
        [ProducesResponseType(typeof(PaginationResponseDto<ColorDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return FromResult(result);
        }

        [HttpGet("{id:int}")]
        [RequirePermission(PermissionCodes.Colors.View)]
        [ProducesResponseType(typeof(ColorDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return FromResult(result);
        }

        [HttpPost]
        [RequirePermission(PermissionCodes.Colors.Create)]
        [ProducesResponseType(typeof(ColorDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateColorDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(dto, creatorId: null, cancellationToken);
            return FromCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
        }

        [HttpPut("{id:int}")]
        [RequirePermission(PermissionCodes.Colors.Update)]
        [ProducesResponseType(typeof(ColorDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateColorDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(id, dto, updaterId: null, cancellationToken);
            return FromResult(result);
        }

        [HttpDelete("{id:int}")]
        [RequirePermission(PermissionCodes.Colors.Delete)]
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
