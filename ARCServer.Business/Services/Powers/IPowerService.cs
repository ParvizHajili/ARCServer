using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Powers;
using ARCServer.Business.Dtos.Common;

namespace ARCServer.Business.Services.Powers
{
    public interface IPowerService
    {
        Task<ServiceResult<PowerDetailDto>> CreateAsync(
            CreatePowerDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PowerDetailDto>> UpdateAsync(
            int id,
            UpdatePowerDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PowerDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PaginationResponseDto<PowerDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> DeleteAsync(
            int id,
            int? deletorId = null,
            CancellationToken cancellationToken = default);
    }
}
