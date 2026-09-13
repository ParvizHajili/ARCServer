using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.Products;

namespace ARCServer.Business.Services.Products
{
    public interface IProductService
    {
        Task<ServiceResult<ProductDetailDto>> CreateAsync(
            CreateProductDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<ProductDetailDto>> UpdateAsync(
            int id,
            UpdateProductDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<ProductDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PaginationResponseDto<ProductListItemDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> DeleteAsync(
            int id,
            int? deletorId = null,
            CancellationToken cancellationToken = default);
    }
}
