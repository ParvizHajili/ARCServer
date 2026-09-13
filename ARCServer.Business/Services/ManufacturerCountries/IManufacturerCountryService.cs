using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.ManufacturerCountries;

namespace ARCServer.Business.Services.ManufacturerCountries
{
    public interface IManufacturerCountryService
    {
        Task<ServiceResult<ManufacturerCountryDetailDto>> CreateAsync(
            CreateManufacturerCountryDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<ManufacturerCountryDetailDto>> UpdateAsync(
            int id,
            UpdateManufacturerCountryDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<ManufacturerCountryDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PaginationResponseDto<ManufacturerCountryDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> DeleteAsync(
            int id,
            int? deletorId = null,
            CancellationToken cancellationToken = default);
    }
}
