using ARCServer.Authorization;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.ManufacturerCountries;
using ARCServer.Business.Services.ManufacturerCountries;
using ARCServer.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Areas.Dashboard.Controllers
{
    [Route("api/dashboard/manufacturer-countries")]
    public class ManufacturerCountriesController : DashboardControllerBase
    {
        private readonly IManufacturerCountryService _service;

        public ManufacturerCountriesController(IManufacturerCountryService service)
        {
            _service = service;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.ManufacturerCountries.List)]
        [ProducesResponseType(typeof(PaginationResponseDto<ManufacturerCountryDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return FromResult(result);
        }

        [HttpGet("{id:int}")]
        [RequirePermission(PermissionCodes.ManufacturerCountries.View)]
        [ProducesResponseType(typeof(ManufacturerCountryDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return FromResult(result);
        }

        [HttpPost]
        [RequirePermission(PermissionCodes.ManufacturerCountries.Create)]
        [ProducesResponseType(typeof(ManufacturerCountryDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateManufacturerCountryDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(dto, creatorId: null, cancellationToken);
            return FromCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
        }

        [HttpPut("{id:int}")]
        [RequirePermission(PermissionCodes.ManufacturerCountries.Update)]
        [ProducesResponseType(typeof(ManufacturerCountryDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateManufacturerCountryDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(id, dto, updaterId: null, cancellationToken);
            return FromResult(result);
        }

        [HttpDelete("{id:int}")]
        [RequirePermission(PermissionCodes.ManufacturerCountries.Delete)]
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
