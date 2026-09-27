using ARCServer.Authorization;
using ARCServer.Business.Dtos.Powers;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Services.Powers;
using ARCServer.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Areas.Dashboard.Controllers
{
    [Route("api/dashboard/powers")]
    public class PowersController : DashboardControllerBase
    {
        private readonly IPowerService _service;

        public PowersController(IPowerService service)
        {
            _service = service;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.Powers.List)]
        [ProducesResponseType(typeof(PaginationResponseDto<PowerDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return FromResult(result);
        }

        [HttpGet("{id:int}")]
        [RequirePermission(PermissionCodes.Powers.View)]
        [ProducesResponseType(typeof(PowerDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return FromResult(result);
        }

        [HttpPost]
        [RequirePermission(PermissionCodes.Powers.Create)]
        [ProducesResponseType(typeof(PowerDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreatePowerDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(dto, creatorId: null, cancellationToken);
            return FromCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
        }

        [HttpPut("{id:int}")]
        [RequirePermission(PermissionCodes.Powers.Update)]
        [ProducesResponseType(typeof(PowerDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdatePowerDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(id, dto, updaterId: null, cancellationToken);
            return FromResult(result);
        }

        [HttpDelete("{id:int}")]
        [RequirePermission(PermissionCodes.Powers.Delete)]
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
