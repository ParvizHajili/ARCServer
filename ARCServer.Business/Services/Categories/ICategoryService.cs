using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Categories;
using ARCServer.Business.Dtos.Common;

namespace ARCServer.Business.Services.Categories
{
    public interface ICategoryService
    {
        Task<ServiceResult<CategoryDetailDto>> CreateAsync(
            CreateCategoryDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<CategoryDetailDto>> UpdateAsync(
            int id,
            UpdateCategoryDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<CategoryDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PaginationResponseDto<CategoryDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> DeleteAsync(
            int id,
            int? deletorId = null,
            CancellationToken cancellationToken = default);
    }
}
