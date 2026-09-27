using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Common.Storage;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.Products;
using ARCServer.Business.Services.Storage;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Products
{
    public class ProductService : BaseService, IProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductTranslation> _translationRepository;
        private readonly IRepository<ProductBrand> _productBrandRepository;
        private readonly IRepository<ProductSize> _productSizeRepository;
        private readonly IRepository<ProductDiameter> _productDiameterRepository;
        private readonly IRepository<ProductPower> _productPowerRepository;
        private readonly IRepository<ProductManufacturerCountry> _productCountryRepository;
        private readonly IRepository<ProductColor> _productColorRepository;
        private readonly IRepository<ProductImage> _productImageRepository;
        private readonly IRepository<ProductSpinImage> _productSpinRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<SubCategory> _subCategoryRepository;
        private readonly IRepository<Brand> _brandRepository;
        private readonly IRepository<Size> _sizeRepository;
        private readonly IRepository<Diameter> _diameterRepository;
        private readonly IRepository<Power> _powerRepository;
        private readonly IRepository<ManufacturerCountry> _countryRepository;
        private readonly IRepository<Color> _colorRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IValidator<CreateProductDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;
        private readonly IValidator<PaginationRequestDto> _paginationValidator;

        public ProductService(
            IRepository<Product> productRepository,
            IRepository<ProductTranslation> translationRepository,
            IRepository<ProductBrand> productBrandRepository,
            IRepository<ProductSize> productSizeRepository,
            IRepository<ProductDiameter> productDiameterRepository,
            IRepository<ProductPower> productPowerRepository,
            IRepository<ProductManufacturerCountry> productCountryRepository,
            IRepository<ProductColor> productColorRepository,
            IRepository<ProductImage> productImageRepository,
            IRepository<ProductSpinImage> productSpinRepository,
            IRepository<Category> categoryRepository,
            IRepository<SubCategory> subCategoryRepository,
            IRepository<Brand> brandRepository,
            IRepository<Size> sizeRepository,
            IRepository<Diameter> diameterRepository,
            IRepository<Power> powerRepository,
            IRepository<ManufacturerCountry> countryRepository,
            IRepository<Color> colorRepository,
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinaryService,
            IValidator<CreateProductDto> createValidator,
            IValidator<UpdateProductDto> updateValidator,
            IValidator<PaginationRequestDto> paginationValidator)
        {
            _productRepository = productRepository;
            _translationRepository = translationRepository;
            _productBrandRepository = productBrandRepository;
            _productSizeRepository = productSizeRepository;
            _productDiameterRepository = productDiameterRepository;
            _productPowerRepository = productPowerRepository;
            _productCountryRepository = productCountryRepository;
            _productColorRepository = productColorRepository;
            _productImageRepository = productImageRepository;
            _productSpinRepository = productSpinRepository;
            _categoryRepository = categoryRepository;
            _subCategoryRepository = subCategoryRepository;
            _brandRepository = brandRepository;
            _sizeRepository = sizeRepository;
            _diameterRepository = diameterRepository;
            _powerRepository = powerRepository;
            _countryRepository = countryRepository;
            _colorRepository = colorRepository;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _paginationValidator = paginationValidator;
        }

        public async Task<ServiceResult<ProductDetailDto>> CreateAsync(
            CreateProductDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<CreateProductDto, ProductDetailDto>(
                _createValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var refsFailure = await ValidateReferencesAsync(dto, cancellationToken);
            if (refsFailure is not null)
            {
                return refsFailure;
            }

            var codeConflict = await FindCodeConflictAsync(dto.Code, excludeProductId: null, cancellationToken);
            if (codeConflict is not null)
            {
                return codeConflict;
            }

            List<(int ColorId, string Url)> uploaded;
            List<string> productImageUrls;
            try
            {
                uploaded = await UploadImagesAsync(dto.Images, dto.ImageColorIds, cancellationToken);
                productImageUrls = await UploadFilesAsync(dto.ProductImages, cancellationToken);
            }
            catch (Exception)
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "images",
                    ErrorMessages.Product.ImageUploadFailed);
            }

            var now = DateTime.UtcNow;
            var product = new Product
            {
                Code = dto.Code.Trim(),
                HasWarranty = dto.HasWarranty,
                IsMadeToOrder = dto.IsMadeToOrder,
                CategoryId = dto.CategoryId,
                SubCategoryId = dto.SubCategoryId,
                CreateDate = now,
                CreatorId = creatorId,
                Deleted = 0,
            };

            foreach (var translation in dto.Translations)
            {
                product.Translations.Add(MapTranslation(translation, now, creatorId));
            }

            foreach (var brandId in dto.BrandIds.Distinct())
            {
                product.Brands.Add(new ProductBrand
                {
                    BrandId = brandId,
                    CreateDate = now,
                    CreatorId = creatorId,
                    Deleted = 0,
                });
            }

            foreach (var sizeId in dto.SizeIds.Distinct())
            {
                product.Sizes.Add(new ProductSize
                {
                    SizeId = sizeId,
                    CreateDate = now,
                    CreatorId = creatorId,
                    Deleted = 0,
                });
            }

            foreach (var diameterId in dto.DiameterIds.Distinct())
            {
                product.Diameters.Add(new ProductDiameter
                {
                    DiameterId = diameterId,
                    CreateDate = now,
                    CreatorId = creatorId,
                    Deleted = 0,
                });
            }

            foreach (var powerId in dto.PowerIds.Distinct())
            {
                product.Powers.Add(new ProductPower
                {
                    PowerId = powerId,
                    CreateDate = now,
                    CreatorId = creatorId,
                    Deleted = 0,
                });
            }

            foreach (var countryId in dto.ManufacturerCountryIds.Distinct())
            {
                product.ManufacturerCountries.Add(new ProductManufacturerCountry
                {
                    ManufacturerCountryId = countryId,
                    CreateDate = now,
                    CreatorId = creatorId,
                    Deleted = 0,
                });
            }

            var imagesByColor = uploaded
                .GroupBy(x => x.ColorId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Url).ToList());

            foreach (var colorId in dto.ColorIds.Distinct())
            {
                var productColor = new ProductColor
                {
                    ColorId = colorId,
                    CreateDate = now,
                    CreatorId = creatorId,
                    Deleted = 0,
                };

                var order = 1;
                foreach (var url in imagesByColor.GetValueOrDefault(colorId) ?? [])
                {
                    var image = new ProductImage
                    {
                        Product = product,
                        ImageUrl = url,
                        Order = order++,
                        CreateDate = now,
                        CreatorId = creatorId,
                        Deleted = 0,
                    };
                    productColor.Images.Add(image);
                    product.Images.Add(image);
                }

                product.Colors.Add(productColor);
            }

            var galleryOrder = 1;
            foreach (var url in productImageUrls)
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = url,
                    Order = galleryOrder++,
                    CreateDate = now,
                    CreatorId = creatorId,
                    Deleted = 0,
                });
            }

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var created = await GetTrackedDetailAsync(product.Id, cancellationToken);
            return ServiceResult<ProductDetailDto>.Success(MapDetail(created!));
        }

        public async Task<ServiceResult<ProductDetailDto>> UpdateAsync(
            int id,
            UpdateProductDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<UpdateProductDto, ProductDetailDto>(
                _updateValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var product = await GetTrackedDetailAsync(id, cancellationToken);
            if (product is null)
            {
                return ServiceResult<ProductDetailDto>.NotFound("id", ErrorMessages.Product.NotFound);
            }

            var refsFailure = await ValidateReferencesAsync(dto, cancellationToken);
            if (refsFailure is not null)
            {
                return refsFailure;
            }

            var codeConflict = await FindCodeConflictAsync(dto.Code, excludeProductId: id, cancellationToken);
            if (codeConflict is not null)
            {
                return codeConflict;
            }

            var existingImages = product.Colors
                .SelectMany(c => c.Images.Where(i => i.Deleted == 0))
                .ToDictionary(i => i.Id);

            var keepIds = dto.KeepImageIds.Distinct().ToHashSet();
            foreach (var keepId in keepIds)
            {
                if (!existingImages.ContainsKey(keepId))
                {
                    return ServiceResult<ProductDetailDto>.Failure(
                        "keepImageIds",
                        ErrorMessages.Format(ErrorMessages.Common.NotFound, ErrorMessages.Fields.Image));
                }
            }

            var keepByColor = existingImages.Values
                .Where(i => keepIds.Contains(i.Id))
                .GroupBy(i => i.ProductColor.ColorId)
                .ToDictionary(g => g.Key, g => g.Count());

            var newByColor = dto.ImageColorIds
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var colorId in dto.ColorIds)
            {
                var keepCount = keepByColor.GetValueOrDefault(colorId);
                var newCount = newByColor.GetValueOrDefault(colorId);
                if (keepCount + newCount < 1)
                {
                    return ServiceResult<ProductDetailDto>.Failure(
                        "images",
                        ErrorMessages.Product.ColorImagesRequired);
                }
            }

            var existingGallery = GalleryImages(product).ToDictionary(i => i.Id);
            var keepProductIds = dto.KeepProductImageIds.Distinct().ToHashSet();
            foreach (var keepId in keepProductIds)
            {
                if (!existingGallery.ContainsKey(keepId))
                {
                    return ServiceResult<ProductDetailDto>.Failure(
                        "keepProductImageIds",
                        ErrorMessages.Format(ErrorMessages.Common.NotFound, ErrorMessages.Fields.Image));
                }
            }

            if (keepProductIds.Count + dto.ProductImages.Count < 1)
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "productImages",
                    ErrorMessages.Product.ProductImagesRequired);
            }

            List<(int ColorId, string Url)> uploaded;
            List<string> productImageUrls;
            try
            {
                uploaded = await UploadImagesAsync(dto.Images, dto.ImageColorIds, cancellationToken);
                productImageUrls = await UploadFilesAsync(dto.ProductImages, cancellationToken);
            }
            catch (Exception)
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "images",
                    ErrorMessages.Product.ImageUploadFailed);
            }

            var now = DateTime.UtcNow;
            product.Code = dto.Code.Trim();
            product.HasWarranty = dto.HasWarranty;
            product.IsMadeToOrder = dto.IsMadeToOrder;
            product.CategoryId = dto.CategoryId;
            product.SubCategoryId = dto.SubCategoryId;
            product.UpdatedDate = now;
            product.UpdaterId = updaterId;

            UpsertTranslations(product, dto.Translations, now, updaterId);
            UpsertBrands(product, dto.BrandIds, now, updaterId);
            UpsertSizes(product, dto.SizeIds, now, updaterId);
            UpsertDiameters(product, dto.DiameterIds, now, updaterId);
            UpsertPowers(product, dto.PowerIds, now, updaterId);
            UpsertCountries(product, dto.ManufacturerCountryIds, now, updaterId);
            UpsertColorsAndImages(product, dto.ColorIds, keepIds, uploaded, now, updaterId);
            UpsertProductImages(product, keepProductIds, productImageUrls, now, updaterId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var refreshed = await GetTrackedDetailAsync(id, cancellationToken);
            return ServiceResult<ProductDetailDto>.Success(MapDetail(refreshed!));
        }

        public async Task<ServiceResult<ProductDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var product = await GetTrackedDetailAsync(id, cancellationToken, asNoTracking: true);
            if (product is null)
            {
                return ServiceResult<ProductDetailDto>.NotFound("id", ErrorMessages.Product.NotFound);
            }

            return ServiceResult<ProductDetailDto>.Success(MapDetail(product));
        }

        public async Task<ServiceResult<PaginationResponseDto<ProductListItemDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var validationFailure =
                await ValidateAsync<PaginationRequestDto, PaginationResponseDto<ProductListItemDto>>(
                    _paginationValidator,
                    request,
                    cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var query = _productRepository.Query()
                .AsNoTracking()
                .Include(p => p.Translations)
                .Include(p => p.Category).ThenInclude(c => c.Translations)
                .Include(p => p.SubCategory)
                .ThenInclude(s => s!.Translations)
                .Include(p => p.Powers).ThenInclude(x => x.Power)
                .AsQueryable();

            var search = request.NormalizedSearch;
            if (search is not null)
            {
                var term = search.ToLower();
                query = request.SearchNameOnly
                    ? query.Where(p =>
                        p.Translations.Any(t =>
                            t.Deleted == 0 && t.Name.ToLower().Contains(term)))
                    : query.Where(p =>
                        p.Code.ToLower().Contains(term)
                        || p.Translations.Any(t =>
                            t.Deleted == 0 && t.Name.ToLower().Contains(term)));
            }

            query = ApplySort(query, request);

            var totalCount = await query.CountAsync(cancellationToken);
            var products = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            IReadOnlyList<ProductListItemDto> items = products.Select(MapListItem).ToList();

            return ServiceResult<PaginationResponseDto<ProductListItemDto>>.Success(
                PaginationResponseDto<ProductListItemDto>.Create(
                    items,
                    request.Page,
                    request.PageSize,
                    totalCount));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(
            int id,
            int? deletorId = null,
            CancellationToken cancellationToken = default)
        {
            var product = await GetTrackedDetailAsync(id, cancellationToken);
            if (product is null)
            {
                return ServiceResult<bool>.NotFound("id", ErrorMessages.Product.NotFound);
            }

            foreach (var translation in product.Translations.Where(t => t.Deleted == 0).ToList())
            {
                _translationRepository.SoftDelete(translation, deletorId);
            }

            foreach (var brand in product.Brands.Where(x => x.Deleted == 0).ToList())
            {
                _productBrandRepository.SoftDelete(brand, deletorId);
            }

            foreach (var size in product.Sizes.Where(x => x.Deleted == 0).ToList())
            {
                _productSizeRepository.SoftDelete(size, deletorId);
            }

            foreach (var diameter in product.Diameters.Where(x => x.Deleted == 0).ToList())
            {
                _productDiameterRepository.SoftDelete(diameter, deletorId);
            }

            foreach (var power in product.Powers.Where(x => x.Deleted == 0).ToList())
            {
                _productPowerRepository.SoftDelete(power, deletorId);
            }

            foreach (var country in product.ManufacturerCountries.Where(x => x.Deleted == 0).ToList())
            {
                _productCountryRepository.SoftDelete(country, deletorId);
            }

            foreach (var color in product.Colors.Where(x => x.Deleted == 0).ToList())
            {
                foreach (var image in color.Images.Where(i => i.Deleted == 0).ToList())
                {
                    _productImageRepository.SoftDelete(image, deletorId);
                }

                _productColorRepository.SoftDelete(color, deletorId);
            }

            foreach (var image in GalleryImages(product).ToList())
            {
                _productImageRepository.SoftDelete(image, deletorId);
            }

            foreach (var image in product.SpinImages.Where(i => i.Deleted == 0).ToList())
            {
                _productSpinRepository.SoftDelete(image, deletorId);
            }

            _productRepository.SoftDelete(product, deletorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<bool>.Success(true);
        }

        private async Task<ServiceResult<ProductDetailDto>?> ValidateReferencesAsync(
            CreateProductDto dto,
            CancellationToken cancellationToken)
        {
            var categoryExists = await _categoryRepository.Query()
                .AnyAsync(c => c.Id == dto.CategoryId, cancellationToken);
            if (!categoryExists)
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "categoryId",
                    ErrorMessages.Product.CategoryNotFound);
            }

            if (dto.SubCategoryId.HasValue)
            {
                var subOk = await _subCategoryRepository.Query()
                    .AnyAsync(
                        s => s.Id == dto.SubCategoryId.Value && s.CategoryId == dto.CategoryId,
                        cancellationToken);
                if (!subOk)
                {
                    return ServiceResult<ProductDetailDto>.Failure(
                        "subCategoryId",
                        ErrorMessages.Product.SubCategoryInvalid);
                }
            }

            var brandCount = await _brandRepository.Query()
                .CountAsync(b => dto.BrandIds.Contains(b.Id), cancellationToken);
            if (brandCount != dto.BrandIds.Distinct().Count())
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "brandIds",
                    ErrorMessages.Product.BrandNotFound);
            }

            var sizeCount = await _sizeRepository.Query()
                .CountAsync(s => dto.SizeIds.Contains(s.Id), cancellationToken);
            if (sizeCount != dto.SizeIds.Distinct().Count())
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "sizeIds",
                    ErrorMessages.Product.SizeNotFound);
            }

            var diameterCount = await _diameterRepository.Query()
                .CountAsync(d => dto.DiameterIds.Contains(d.Id), cancellationToken);
            if (diameterCount != dto.DiameterIds.Distinct().Count())
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "diameterIds",
                    ErrorMessages.Product.DiameterNotFound);
            }

            var powerCount = await _powerRepository.Query()
                .CountAsync(p => dto.PowerIds.Contains(p.Id), cancellationToken);
            if (powerCount != dto.PowerIds.Distinct().Count())
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "powerIds",
                    ErrorMessages.Product.PowerNotFound);
            }

            var countryCount = await _countryRepository.Query()
                .CountAsync(c => dto.ManufacturerCountryIds.Contains(c.Id), cancellationToken);
            if (countryCount != dto.ManufacturerCountryIds.Distinct().Count())
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "manufacturerCountryIds",
                    ErrorMessages.Product.ManufacturerCountryNotFound);
            }

            var colorCount = await _colorRepository.Query()
                .CountAsync(c => dto.ColorIds.Contains(c.Id), cancellationToken);
            if (colorCount != dto.ColorIds.Distinct().Count())
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "colorIds",
                    ErrorMessages.Product.ColorNotFound);
            }

            if (dto.ImageColorIds.Any(id => !dto.ColorIds.Contains(id)))
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "imageColorIds",
                    ErrorMessages.Product.ImageColorIdsMismatch);
            }

            return null;
        }

        private async Task<ServiceResult<ProductDetailDto>?> FindCodeConflictAsync(
            string code,
            int? excludeProductId,
            CancellationToken cancellationToken)
        {
            var normalized = code.Trim().ToLowerInvariant();
            var query = _productRepository.Query()
                .Where(p => p.Code.ToLower() == normalized);

            if (excludeProductId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProductId.Value);
            }

            if (await query.AnyAsync(cancellationToken))
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "code",
                    ErrorMessages.Format(ErrorMessages.Product.CodeExists, code.Trim()),
                    ServiceErrorType.Conflict);
            }

            return null;
        }

        private async Task<List<(int ColorId, string Url)>> UploadImagesAsync(
            List<IFormFile> images,
            List<int> imageColorIds,
            CancellationToken cancellationToken)
        {
            var result = new List<(int ColorId, string Url)>();
            for (var i = 0; i < images.Count; i++)
            {
                var url = await _cloudinaryService.UploadAsync(
                    images[i],
                    CloudinaryFolders.Products,
                    cancellationToken);
                result.Add((imageColorIds[i], url));
            }

            return result;
        }

        private async Task<List<string>> UploadFilesAsync(
            List<IFormFile> images,
            CancellationToken cancellationToken)
        {
            var result = new List<string>();
            foreach (var file in images)
            {
                var url = await _cloudinaryService.UploadAsync(
                    file,
                    CloudinaryFolders.Products,
                    cancellationToken);
                result.Add(url);
            }

            return result;
        }

        private async Task<Product?> GetTrackedDetailAsync(
            int id,
            CancellationToken cancellationToken,
            bool asNoTracking = false)
        {
            var query = _productRepository.Query()
                .Include(p => p.Translations)
                .Include(p => p.Category).ThenInclude(c => c.Translations)
                .Include(p => p.SubCategory)
                .ThenInclude(s => s!.Translations)
                .Include(p => p.Brands).ThenInclude(b => b.Brand).ThenInclude(b => b.Translations)
                .Include(p => p.Sizes).ThenInclude(s => s.Size)
                .Include(p => p.Diameters).ThenInclude(d => d.Diameter)
                .Include(p => p.Powers).ThenInclude(pw => pw.Power)
                .Include(p => p.ManufacturerCountries)
                    .ThenInclude(m => m.ManufacturerCountry)
                    .ThenInclude(c => c.Translations)
                .Include(p => p.Colors).ThenInclude(c => c.Color).ThenInclude(c => c.Translations)
                .Include(p => p.Colors).ThenInclude(c => c.Images)
                .Include(p => p.Images)
                .Include(p => p.SpinImages)
                .Where(p => p.Id == id);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        private void UpsertTranslations(
            Product product,
            List<ProductTranslationInputDto> translations,
            DateTime now,
            int? userId)
        {
            var incomingCodes = translations
                .Select(t => t.LanguageCode.Trim().ToLowerInvariant())
                .ToHashSet();

            foreach (var existing in product.Translations.ToList())
            {
                if (!incomingCodes.Contains(existing.LanguageCode))
                {
                    _translationRepository.SoftDelete(existing, userId);
                }
            }

            foreach (var translation in translations)
            {
                var code = translation.LanguageCode.Trim().ToLowerInvariant();
                var existing = product.Translations.FirstOrDefault(x => x.LanguageCode == code);
                if (existing is null)
                {
                    product.Translations.Add(MapTranslation(translation, now, userId));
                    continue;
                }

                existing.Name = translation.Name.Trim();
                existing.Description = translation.Description?.Trim() ?? string.Empty;
                existing.UpdatedDate = now;
                existing.UpdaterId = userId;
            }
        }

        private void UpsertBrands(Product product, List<int> brandIds, DateTime now, int? userId)
        {
            var incoming = brandIds.Distinct().ToHashSet();
            foreach (var existing in product.Brands.Where(x => x.Deleted == 0).ToList())
            {
                if (!incoming.Contains(existing.BrandId))
                {
                    _productBrandRepository.SoftDelete(existing, userId);
                }
            }

            foreach (var brandId in incoming)
            {
                if (product.Brands.Any(x => x.BrandId == brandId && x.Deleted == 0))
                {
                    continue;
                }

                var softDeleted = product.Brands.FirstOrDefault(x => x.BrandId == brandId);
                if (softDeleted is not null)
                {
                    softDeleted.Deleted = 0;
                    softDeleted.DeletedDate = null;
                    softDeleted.DeletorId = null;
                    softDeleted.UpdatedDate = now;
                    softDeleted.UpdaterId = userId;
                    continue;
                }

                product.Brands.Add(new ProductBrand
                {
                    BrandId = brandId,
                    CreateDate = now,
                    CreatorId = userId,
                    Deleted = 0,
                });
            }
        }

        private void UpsertSizes(Product product, List<int> sizeIds, DateTime now, int? userId)
        {
            var incoming = sizeIds.Distinct().ToHashSet();
            foreach (var existing in product.Sizes.Where(x => x.Deleted == 0).ToList())
            {
                if (!incoming.Contains(existing.SizeId))
                {
                    _productSizeRepository.SoftDelete(existing, userId);
                }
            }

            foreach (var sizeId in incoming)
            {
                if (product.Sizes.Any(x => x.SizeId == sizeId && x.Deleted == 0))
                {
                    continue;
                }

                var softDeleted = product.Sizes.FirstOrDefault(x => x.SizeId == sizeId);
                if (softDeleted is not null)
                {
                    softDeleted.Deleted = 0;
                    softDeleted.DeletedDate = null;
                    softDeleted.DeletorId = null;
                    softDeleted.UpdatedDate = now;
                    softDeleted.UpdaterId = userId;
                    continue;
                }

                product.Sizes.Add(new ProductSize
                {
                    SizeId = sizeId,
                    CreateDate = now,
                    CreatorId = userId,
                    Deleted = 0,
                });
            }
        }

        private void UpsertDiameters(Product product, List<int> diameterIds, DateTime now, int? userId)
        {
            var incoming = diameterIds.Distinct().ToHashSet();
            foreach (var existing in product.Diameters.Where(x => x.Deleted == 0).ToList())
            {
                if (!incoming.Contains(existing.DiameterId))
                {
                    _productDiameterRepository.SoftDelete(existing, userId);
                }
            }

            foreach (var diameterId in incoming)
            {
                if (product.Diameters.Any(x => x.DiameterId == diameterId && x.Deleted == 0))
                {
                    continue;
                }

                var softDeleted = product.Diameters.FirstOrDefault(x => x.DiameterId == diameterId);
                if (softDeleted is not null)
                {
                    softDeleted.Deleted = 0;
                    softDeleted.DeletedDate = null;
                    softDeleted.DeletorId = null;
                    softDeleted.UpdatedDate = now;
                    softDeleted.UpdaterId = userId;
                    continue;
                }

                product.Diameters.Add(new ProductDiameter
                {
                    DiameterId = diameterId,
                    CreateDate = now,
                    CreatorId = userId,
                    Deleted = 0,
                });
            }
        }

        private void UpsertPowers(Product product, List<int> powerIds, DateTime now, int? userId)
        {
            var incoming = powerIds.Distinct().ToHashSet();
            foreach (var existing in product.Powers.Where(x => x.Deleted == 0).ToList())
            {
                if (!incoming.Contains(existing.PowerId))
                {
                    _productPowerRepository.SoftDelete(existing, userId);
                }
            }

            foreach (var powerId in incoming)
            {
                if (product.Powers.Any(x => x.PowerId == powerId && x.Deleted == 0))
                {
                    continue;
                }

                var softDeleted = product.Powers.FirstOrDefault(x => x.PowerId == powerId);
                if (softDeleted is not null)
                {
                    softDeleted.Deleted = 0;
                    softDeleted.DeletedDate = null;
                    softDeleted.DeletorId = null;
                    softDeleted.UpdatedDate = now;
                    softDeleted.UpdaterId = userId;
                    continue;
                }

                product.Powers.Add(new ProductPower
                {
                    PowerId = powerId,
                    CreateDate = now,
                    CreatorId = userId,
                    Deleted = 0,
                });
            }
        }

        private void UpsertCountries(Product product, List<int> countryIds, DateTime now, int? userId)
        {
            var incoming = countryIds.Distinct().ToHashSet();
            foreach (var existing in product.ManufacturerCountries.Where(x => x.Deleted == 0).ToList())
            {
                if (!incoming.Contains(existing.ManufacturerCountryId))
                {
                    _productCountryRepository.SoftDelete(existing, userId);
                }
            }

            foreach (var countryId in incoming)
            {
                if (product.ManufacturerCountries.Any(x =>
                        x.ManufacturerCountryId == countryId && x.Deleted == 0))
                {
                    continue;
                }

                var softDeleted = product.ManufacturerCountries
                    .FirstOrDefault(x => x.ManufacturerCountryId == countryId);
                if (softDeleted is not null)
                {
                    softDeleted.Deleted = 0;
                    softDeleted.DeletedDate = null;
                    softDeleted.DeletorId = null;
                    softDeleted.UpdatedDate = now;
                    softDeleted.UpdaterId = userId;
                    continue;
                }

                product.ManufacturerCountries.Add(new ProductManufacturerCountry
                {
                    ManufacturerCountryId = countryId,
                    CreateDate = now,
                    CreatorId = userId,
                    Deleted = 0,
                });
            }
        }

        private void UpsertColorsAndImages(
            Product product,
            List<int> colorIds,
            HashSet<int> keepImageIds,
            List<(int ColorId, string Url)> uploaded,
            DateTime now,
            int? userId)
        {
            var incomingColors = colorIds.Distinct().ToHashSet();

            foreach (var existing in product.Colors.Where(x => x.Deleted == 0).ToList())
            {
                if (!incomingColors.Contains(existing.ColorId))
                {
                    foreach (var image in existing.Images.Where(i => i.Deleted == 0).ToList())
                    {
                        _productImageRepository.SoftDelete(image, userId);
                    }

                    _productColorRepository.SoftDelete(existing, userId);
                }
            }

            foreach (var colorId in incomingColors)
            {
                var productColor = product.Colors.FirstOrDefault(x => x.ColorId == colorId && x.Deleted == 0);
                if (productColor is null)
                {
                    var softDeleted = product.Colors.FirstOrDefault(x => x.ColorId == colorId);
                    if (softDeleted is not null)
                    {
                        softDeleted.Deleted = 0;
                        softDeleted.DeletedDate = null;
                        softDeleted.DeletorId = null;
                        softDeleted.UpdatedDate = now;
                        softDeleted.UpdaterId = userId;
                        productColor = softDeleted;
                    }
                    else
                    {
                        productColor = new ProductColor
                        {
                            ColorId = colorId,
                            CreateDate = now,
                            CreatorId = userId,
                            Deleted = 0,
                        };
                        product.Colors.Add(productColor);
                    }
                }

                foreach (var image in productColor.Images.Where(i => i.Deleted == 0).ToList())
                {
                    if (!keepImageIds.Contains(image.Id))
                    {
                        _productImageRepository.SoftDelete(image, userId);
                    }
                }

                var nextOrder = productColor.Images
                    .Where(i => i.Deleted == 0 && keepImageIds.Contains(i.Id))
                    .Select(i => i.Order)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                foreach (var (_, url) in uploaded.Where(u => u.ColorId == colorId))
                {
                    var image = new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = url,
                        Order = nextOrder++,
                        CreateDate = now,
                        CreatorId = userId,
                        Deleted = 0,
                    };
                    productColor.Images.Add(image);
                    product.Images.Add(image);
                }
            }
        }

        private void UpsertProductImages(
            Product product,
            HashSet<int> keepImageIds,
            List<string> uploadedUrls,
            DateTime now,
            int? userId)
        {
            foreach (var image in GalleryImages(product).ToList())
            {
                if (!keepImageIds.Contains(image.Id))
                {
                    _productImageRepository.SoftDelete(image, userId);
                }
            }

            var nextOrder = GalleryImages(product)
                .Where(i => keepImageIds.Contains(i.Id))
                .Select(i => i.Order)
                .DefaultIfEmpty(0)
                .Max() + 1;

            foreach (var url in uploadedUrls)
            {
                product.Images.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = url,
                    Order = nextOrder++,
                    CreateDate = now,
                    CreatorId = userId,
                    Deleted = 0,
                });
            }
        }

        private static IEnumerable<ProductImage> GalleryImages(Product product)
        {
            return product.Images.Where(i =>
                i.Deleted == 0 && i.ProductColorId == null && i.ProductColor == null);
        }

        private static ProductTranslation MapTranslation(
            ProductTranslationInputDto translation,
            DateTime now,
            int? creatorId)
        {
            return new ProductTranslation
            {
                LanguageCode = translation.LanguageCode.Trim().ToLowerInvariant(),
                Name = translation.Name.Trim(),
                Description = translation.Description?.Trim() ?? string.Empty,
                CreateDate = now,
                CreatorId = creatorId,
                Deleted = 0,
            };
        }

        private static IQueryable<Product> ApplySort(
            IQueryable<Product> query,
            PaginationRequestDto request)
        {
            var desc = request.IsDescending;
            return request.NormalizedSortBy switch
            {
                "id" => desc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id),
                "code" => desc ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code),
                _ => desc
                    ? query.OrderByDescending(p => p.Translations
                        .Where(t => t.LanguageCode == "az" && t.Deleted == 0)
                        .Select(t => t.Name)
                        .FirstOrDefault())
                    : query.OrderBy(p => p.Translations
                        .Where(t => t.LanguageCode == "az" && t.Deleted == 0)
                        .Select(t => t.Name)
                        .FirstOrDefault()),
            };
        }

        private static string GetAzName(IEnumerable<(string LanguageCode, string Name)> translations)
        {
            var list = translations.ToList();
            var az = list.FirstOrDefault(t => t.LanguageCode == "az");
            if (!string.IsNullOrWhiteSpace(az.Name))
            {
                return az.Name;
            }

            return list.Select(t => t.Name).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                ?? string.Empty;
        }

        private static string NameFromTranslations(
            ICollection<CategoryTranslation> translations) =>
            GetAzName(translations.Select(t => (t.LanguageCode, t.Name)));

        private static string NameFromTranslations(
            ICollection<SubCategoryTranslation> translations) =>
            GetAzName(translations.Select(t => (t.LanguageCode, t.Name)));

        private static string NameFromTranslations(
            ICollection<BrandTranslation> translations) =>
            GetAzName(translations.Select(t => (t.LanguageCode, t.Name)));

        private static string NameFromTranslations(
            ICollection<ManufacturerCountryTranslation> translations) =>
            GetAzName(translations.Select(t => (t.LanguageCode, t.Name)));

        private static string NameFromTranslations(
            ICollection<ColorTranslation> translations) =>
            GetAzName(translations.Select(t => (t.LanguageCode, t.Name)));

        private static ProductDetailDto MapDetail(Product product)
        {
            return new ProductDetailDto
            {
                Id = product.Id,
                Code = product.Code,
                HasWarranty = product.HasWarranty,
                IsMadeToOrder = product.IsMadeToOrder,
                CategoryId = product.CategoryId,
                CategoryName = NameFromTranslations(product.Category.Translations),
                SubCategoryId = product.SubCategoryId,
                SubCategoryName = product.SubCategory is null
                    ? null
                    : NameFromTranslations(product.SubCategory.Translations),
                Translations = product.Translations
                    .Where(t => t.Deleted == 0)
                    .OrderBy(t => t.LanguageCode)
                    .Select(t => new ProductTranslationResultDto
                    {
                        LanguageCode = t.LanguageCode,
                        Name = t.Name,
                        Description = t.Description,
                    })
                    .ToList(),
                Brands = product.Brands
                    .Where(b => b.Deleted == 0)
                    .Select(b => new ProductNamedRefDto
                    {
                        Id = b.BrandId,
                        Name = NameFromTranslations(b.Brand.Translations),
                    })
                    .OrderBy(b => b.Name)
                    .ToList(),
                Sizes = MapNamedRefs(
                    product.Sizes.Where(x => x.Deleted == 0),
                    x => x.SizeId,
                    x => FormatDecimal(x.Size.Value)),
                Diameters = MapNamedRefs(
                    product.Diameters.Where(x => x.Deleted == 0),
                    x => x.DiameterId,
                    x => FormatDecimal(x.Diameter.Value)),
                Powers = MapNamedRefs(
                    product.Powers.Where(x => x.Deleted == 0),
                    x => x.PowerId,
                    x => FormatDecimal(x.Power.Value)),
                ManufacturerCountries = product.ManufacturerCountries
                    .Where(m => m.Deleted == 0)
                    .Select(m => new ProductNamedRefDto
                    {
                        Id = m.ManufacturerCountryId,
                        Name = NameFromTranslations(m.ManufacturerCountry.Translations),
                    })
                    .OrderBy(m => m.Name)
                    .ToList(),
                Images = GalleryImages(product)
                    .OrderBy(i => i.Order)
                    .Select(i => new ProductImageDetailDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        Order = i.Order,
                    })
                    .ToList(),
                Colors = product.Colors
                    .Where(c => c.Deleted == 0)
                    .Select(c => new ProductColorDetailDto
                    {
                        ColorId = c.ColorId,
                        HexCode = c.Color.HexCode,
                        Name = NameFromTranslations(c.Color.Translations),
                        Images = c.Images
                            .Where(i => i.Deleted == 0)
                            .OrderBy(i => i.Order)
                            .Select(i => new ProductImageDetailDto
                            {
                                Id = i.Id,
                                ImageUrl = i.ImageUrl,
                                Order = i.Order,
                            })
                            .ToList(),
                    })
                    .OrderBy(c => c.Name)
                    .ToList(),
            };
        }

        private static string FormatDecimal(decimal value) =>
            value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

        private static List<ProductNamedRefDto> MapNamedRefs<T>(
            IEnumerable<T> items,
            Func<T, int> idSelector,
            Func<T, string> nameSelector)
        {
            return items
                .Select(item => new ProductNamedRefDto
                {
                    Id = idSelector(item),
                    Name = nameSelector(item),
                })
                .OrderBy(x => x.Name)
                .ToList();
        }

        private static ProductListItemDto MapListItem(Product product)
        {
            return new ProductListItemDto
            {
                Id = product.Id,
                Code = product.Code,
                Name = product.Translations
                    .Where(t => t.Deleted == 0)
                    .OrderBy(t => t.LanguageCode == "az" ? 0 : 1)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? string.Empty,
                CategoryName = NameFromTranslations(product.Category.Translations),
                SubCategoryName = product.SubCategory is null
                    ? null
                    : NameFromTranslations(product.SubCategory.Translations),
                Powers = string.Join(
                    ", ",
                    product.Powers
                        .Where(x => x.Deleted == 0)
                        .Select(x => FormatDecimal(x.Power.Value))
                        .Where(name => name.Length > 0)
                        .OrderBy(name => name)),
                HasWarranty = product.HasWarranty,
                IsMadeToOrder = product.IsMadeToOrder,
            };
        }
    }
}
