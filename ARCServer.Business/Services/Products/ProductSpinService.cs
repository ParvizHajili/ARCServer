using System.Text.Json;
using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Common.Storage;
using ARCServer.Business.Dtos.Products;
using ARCServer.Business.Services.Storage;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Products
{
    public class ProductSpinService : IProductSpinService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductSpinImage> _spinRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;

        public ProductSpinService(
            IRepository<Product> productRepository,
            IRepository<ProductSpinImage> spinRepository,
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinaryService)
        {
            _productRepository = productRepository;
            _spinRepository = spinRepository;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<ServiceResult<ProductSpinDto>> GetAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            var product = await LoadAsync(productId, asNoTracking: true, cancellationToken);
            if (product is null)
            {
                return ServiceResult<ProductSpinDto>.NotFound("id", ErrorMessages.Product.NotFound);
            }

            return ServiceResult<ProductSpinDto>.Success(Map(product));
        }

        public async Task<ServiceResult<ProductSpinDto>> SaveAsync(
            int productId,
            ProductSpinFormRequest request,
            int? userId = null,
            CancellationToken cancellationToken = default)
        {
            var product = await LoadAsync(productId, asNoTracking: false, cancellationToken);
            if (product is null)
            {
                return ServiceResult<ProductSpinDto>.NotFound("id", ErrorMessages.Product.NotFound);
            }

            if (!TryParseKeys(request.FrameKeys, out var keys))
            {
                return ServiceResult<ProductSpinDto>.Failure(
                    "frameKeys",
                    ErrorMessages.Product.InvalidSpinFrames);
            }

            var files = request.Images?
                .Where(f => f is { Length: > 0 })
                .ToList() ?? [];
            var newCount = keys.Count(k => k.IsNew);
            if (newCount != files.Count || keys.Where(k => k.IsNew).Select(k => k.Index).Distinct().Count() != newCount)
            {
                return ServiceResult<ProductSpinDto>.Failure(
                    "frameKeys",
                    ErrorMessages.Product.InvalidSpinFrames);
            }

            if (keys.Where(k => k.IsNew).Any(k => k.Index < 0 || k.Index >= files.Count))
            {
                return ServiceResult<ProductSpinDto>.Failure(
                    "frameKeys",
                    ErrorMessages.Product.InvalidSpinFrames);
            }

            var existing = product.SpinImages.Where(i => i.Deleted == 0).ToDictionary(i => i.Id);
            var seen = new HashSet<int>();
            foreach (var key in keys.Where(k => !k.IsNew))
            {
                if (!existing.ContainsKey(key.Index) || !seen.Add(key.Index))
                {
                    return ServiceResult<ProductSpinDto>.Failure(
                        "frameKeys",
                        ErrorMessages.Product.InvalidSpinFrames);
                }
            }

            List<string> uploaded;
            try
            {
                uploaded = [];
                foreach (var file in files)
                {
                    uploaded.Add(await _cloudinaryService.UploadAsync(
                        file,
                        CloudinaryFolders.ProductSpins,
                        cancellationToken));
                }
            }
            catch (Exception)
            {
                return ServiceResult<ProductSpinDto>.Failure(
                    "images",
                    ErrorMessages.Product.ImageUploadFailed);
            }

            var now = DateTime.UtcNow;
            foreach (var image in existing.Values.Where(i => !seen.Contains(i.Id)).ToList())
            {
                _spinRepository.SoftDelete(image, userId);
            }

            var order = 1;
            foreach (var key in keys)
            {
                if (key.IsNew)
                {
                    product.SpinImages.Add(new ProductSpinImage
                    {
                        ImageUrl = uploaded[key.Index],
                        Order = order++,
                        CreateDate = now,
                        CreatorId = userId,
                        Deleted = 0,
                    });
                    continue;
                }

                var image = existing[key.Index];
                image.Order = order++;
                image.UpdatedDate = now;
                image.UpdaterId = userId;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var refreshed = await LoadAsync(productId, asNoTracking: true, cancellationToken);
            return ServiceResult<ProductSpinDto>.Success(Map(refreshed!));
        }

        private async Task<Product?> LoadAsync(
            int productId,
            bool asNoTracking,
            CancellationToken cancellationToken)
        {
            var query = _productRepository.Query()
                .Include(p => p.Translations)
                .Include(p => p.SpinImages)
                .Where(p => p.Id == productId);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        private static bool TryParseKeys(string? json, out List<(bool IsNew, int Index)> keys)
        {
            keys = [];
            if (string.IsNullOrWhiteSpace(json))
            {
                return true;
            }

            List<string>? raw;
            try
            {
                raw = JsonSerializer.Deserialize<List<string>>(json);
            }
            catch (JsonException)
            {
                return false;
            }

            foreach (var item in raw ?? [])
            {
                var parts = item.Split(':', 2);
                if (parts.Length != 2 || !int.TryParse(parts[1], out var index))
                {
                    return false;
                }

                if (parts[0] == "e")
                {
                    keys.Add((false, index));
                }
                else if (parts[0] == "n")
                {
                    keys.Add((true, index));
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static ProductSpinDto Map(Product product)
        {
            var az = product.Translations
                .Where(t => t.Deleted == 0)
                .OrderBy(t => t.LanguageCode == "az" ? 0 : 1)
                .Select(t => t.Name)
                .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));

            return new ProductSpinDto
            {
                ProductId = product.Id,
                Code = product.Code,
                Name = az ?? string.Empty,
                Images = product.SpinImages
                    .Where(i => i.Deleted == 0)
                    .OrderBy(i => i.Order)
                    .Select(i => new ProductSpinImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        Order = i.Order,
                    })
                    .ToList(),
            };
        }
    }
}
