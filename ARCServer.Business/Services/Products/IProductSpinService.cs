using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Products;

namespace ARCServer.Business.Services.Products
{
    public interface IProductSpinService
    {
        Task<ServiceResult<ProductSpinDto>> GetAsync(
            int productId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<ProductSpinDto>> SaveAsync(
            int productId,
            ProductSpinFormRequest request,
            int? userId = null,
            CancellationToken cancellationToken = default);
    }
}
