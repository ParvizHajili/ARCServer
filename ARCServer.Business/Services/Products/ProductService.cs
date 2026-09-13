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
        private readonly IRepository<ProductManufacturerCountry> _productCountryRepository;
        private readonly IRepository<ProductColor> _productColorRepository;
        private readonly IRepository<ProductImage> _productImageRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<SubCategory> _subCategoryRepository;
        private readonly IRepository<Brand> _brandRepository;
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
            IRepository<ProductManufacturerCountry> productCountryRepository,
            IRepository<ProductColor> productColorRepository,
            IRepository<ProductImage> productImageRepository,
            IRepository<Category> categoryRepository,
            IRepository<SubCategory> subCategoryRepository,
            IRepository<Brand> brandRepository,
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
            _productCountryRepository = productCountryRepository;
            _productColorRepository = productColorRepository;
            _productImageRepository = productImageRepository;
            _categoryRepository = categoryRepository;
            _subCategoryRepository = subCategoryRepository;
            _brandRepository = brandRepository;
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
            try
            {
                uploaded = await UploadImagesAsync(dto.Images, dto.ImageColorIds, cancellationToken);
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
                Size = dto.Size.Trim(),
                Diameter = dto.Diameter.Trim(),
                HasWarranty = dto.HasWarranty,
                IsMadeToOrder = dto.IsMadeToOrder,
                PowerAmperes = dto.PowerAmperes,
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
                    productColor.Images.Add(new ProductImage
                    {
                        ImageUrl = url,
                        Order = order++,
                        CreateDate = now,
                        CreatorId = creatorId,
                        Deleted = 0,
                    });
                }

                product.Colors.Add(productColor);
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

            List<(int ColorId, string Url)> uploaded;
            try
            {
                uploaded = await UploadImagesAsync(dto.Images, dto.ImageColorIds, cancellationToken);
            }
            catch (Exception)
            {
                return ServiceResult<ProductDetailDto>.Failure(
                    "images",
                    ErrorMessages.Product.ImageUploadFailed);
            }

            var now = DateTime.UtcNow;
            product.Code = dto.Code.Trim();
            product.Size = dto.Size.Trim();
            product.Diameter = dto.Diameter.Trim();
            product.HasWarranty = dto.HasWarranty;
            product.IsMadeToOrder = dto.IsMadeToOrder;
            product.PowerAmperes = dto.PowerAmperes;
            product.CategoryId = dto.CategoryId;
            product.SubCategoryId = dto.SubCategoryId;
            product.UpdatedDate = now;
            product.UpdaterId = updaterId;

            UpsertTranslations(product, dto.Translations, now, updaterId);
            UpsertBrands(product, dto.BrandIds, now, updaterId);
            UpsertCountries(product, dto.ManufacturerCountryIds, now, updaterId);
            UpsertColorsAndImages(product, dto.ColorIds, keepIds, uploaded, now, updaterId);

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
                .AsQueryable();

            var search = request.NormalizedSearch;
            if (search is not null)
            {
                query = query.Where(p =>
                    p.Code.ToLower().Contains(search.ToLower())
                    || p.Translations.Any(t =>
                        t.Deleted == 0 && t.Name.ToLower().Contains(search.ToLower())));
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
                .Include(p => p.ManufacturerCountries)
                    .ThenInclude(m => m.ManufacturerCountry)
                    .ThenInclude(c => c.Translations)
                .Include(p => p.Colors).ThenInclude(c => c.Color).ThenInclude(c => c.Translations)
                .Include(p => p.Colors).ThenInclude(c => c.Images)
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
                    productColor.Images.Add(new ProductImage
                    {
                        ImageUrl = url,
                        Order = nextOrder++,
                        CreateDate = now,
                        CreatorId = userId,
                        Deleted = 0,
                    });
                }
            }
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
                Size = product.Size,
                Diameter = product.Diameter,
                HasWarranty = product.HasWarranty,
                IsMadeToOrder = product.IsMadeToOrder,
                PowerAmperes = product.PowerAmperes,
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
                ManufacturerCountries = product.ManufacturerCountries
                    .Where(m => m.Deleted == 0)
                    .Select(m => new ProductNamedRefDto
                    {
                        Id = m.ManufacturerCountryId,
                        Name = NameFromTranslations(m.ManufacturerCountry.Translations),
                    })
                    .OrderBy(m => m.Name)
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
                PowerAmperes = product.PowerAmperes,
                HasWarranty = product.HasWarranty,
                IsMadeToOrder = product.IsMadeToOrder,
            };
        }
    }
}
