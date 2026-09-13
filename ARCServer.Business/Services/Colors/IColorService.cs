using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Colors;
using ARCServer.Business.Dtos.Common;

namespace ARCServer.Business.Services.Colors
{
    public interface IColorService
    {
        Task<ServiceResult<ColorDetailDto>> CreateAsync(
            CreateColorDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<ColorDetailDto>> UpdateAsync(
            int id,
            UpdateColorDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<ColorDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PaginationResponseDto<ColorDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> DeleteAsync(
            int id,
            int? deletorId = null,
            CancellationToken cancellationToken = default);
    }
}
