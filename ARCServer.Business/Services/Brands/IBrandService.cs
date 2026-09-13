using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Brands;
using ARCServer.Business.Dtos.Common;

namespace ARCServer.Business.Services.Brands
{
    public interface IBrandService
    {
        Task<ServiceResult<BrandDetailDto>> CreateAsync(
            CreateBrandDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<BrandDetailDto>> UpdateAsync(
            int id,
            UpdateBrandDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<BrandDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PaginationResponseDto<BrandDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> DeleteAsync(
            int id,
            int? deletorId = null,
            CancellationToken cancellationToken = default);
    }
}
