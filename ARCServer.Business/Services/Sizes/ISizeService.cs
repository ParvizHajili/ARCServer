using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Sizes;
using ARCServer.Business.Dtos.Common;

namespace ARCServer.Business.Services.Sizes
{
    public interface ISizeService
    {
        Task<ServiceResult<SizeDetailDto>> CreateAsync(
            CreateSizeDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<SizeDetailDto>> UpdateAsync(
            int id,
            UpdateSizeDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<SizeDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PaginationResponseDto<SizeDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> DeleteAsync(
            int id,
            int? deletorId = null,
            CancellationToken cancellationToken = default);
    }
}
