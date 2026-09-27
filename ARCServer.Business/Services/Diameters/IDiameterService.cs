using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Diameters;
using ARCServer.Business.Dtos.Common;

namespace ARCServer.Business.Services.Diameters
{
    public interface IDiameterService
    {
        Task<ServiceResult<DiameterDetailDto>> CreateAsync(
            CreateDiameterDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<DiameterDetailDto>> UpdateAsync(
            int id,
            UpdateDiameterDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<DiameterDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PaginationResponseDto<DiameterDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> DeleteAsync(
            int id,
            int? deletorId = null,
            CancellationToken cancellationToken = default);
    }
}
