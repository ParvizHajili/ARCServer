using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Common.Storage;
using ARCServer.Business.Dtos.Categories;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Services.Storage;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Categories
{
    public class CategoryService : BaseService, ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<SubCategory> _subCategoryRepository;
        private readonly IRepository<CategoryTranslation> _categoryTranslationRepository;
        private readonly IRepository<SubCategoryTranslation> _subCategoryTranslationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateCategoryDto> _createValidator;
        private readonly IValidator<UpdateCategoryDto> _updateValidator;
        private readonly IValidator<PaginationRequestDto> _paginationValidator;

        public CategoryService(
            IRepository<Category> categoryRepository,
            IRepository<SubCategory> subCategoryRepository,
            IRepository<CategoryTranslation> categoryTranslationRepository,
            IRepository<SubCategoryTranslation> subCategoryTranslationRepository,
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinaryService,
            IMapper mapper,
            IValidator<CreateCategoryDto> createValidator,
            IValidator<UpdateCategoryDto> updateValidator,
            IValidator<PaginationRequestDto> paginationValidator)
        {
            _categoryRepository = categoryRepository;
            _subCategoryRepository = subCategoryRepository;
            _categoryTranslationRepository = categoryTranslationRepository;
            _subCategoryTranslationRepository = subCategoryTranslationRepository;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _paginationValidator = paginationValidator;
        }

        public async Task<ServiceResult<CategoryDetailDto>> CreateAsync(
            CreateCategoryDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<CreateCategoryDto, CategoryDetailDto>(
                _createValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var orderExists = await _categoryRepository.Query()
                .AnyAsync(x => x.Order == dto.Order, cancellationToken);

            if (orderExists)
            {
                return ServiceResult<CategoryDetailDto>.Failure(
                    "order",
                    ErrorMessages.Format(ErrorMessages.Category.OrderExists, dto.Order),
                    ServiceErrorType.Conflict);
            }

            string imageUrl;
            try
            {
                imageUrl = await _cloudinaryService.UploadAsync(
                    dto.Image!,
                    CloudinaryFolders.Categories,
                    cancellationToken);
            }
            catch (Exception)
            {
                return ServiceResult<CategoryDetailDto>.Failure(
                    "image",
                    ErrorMessages.Category.ImageUploadFailed);
            }

            var now = DateTime.UtcNow;

            var category = new Category
            {
                Image = imageUrl,
                Order = dto.Order,
                CreateDate = now,
                CreatorId = creatorId,
                Deleted = 0,
            };

            foreach (var translation in dto.Translations)
            {
                category.Translations.Add(MapCategoryTranslation(translation, now, creatorId));
            }

            foreach (var subDto in dto.SubCategories)
            {
                category.SubCategories.Add(MapSubCategory(subDto.Translations, now, creatorId));
            }

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<CategoryDetailDto>.Success(_mapper.Map<CategoryDetailDto>(category));
        }

        public async Task<ServiceResult<CategoryDetailDto>> UpdateAsync(
            int id,
            UpdateCategoryDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<UpdateCategoryDto, CategoryDetailDto>(
                _updateValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var category = await GetTrackedCategoryAsync(id, cancellationToken);
            if (category is null)
            {
                return ServiceResult<CategoryDetailDto>.NotFound("id", ErrorMessages.Category.NotFound);
            }

            var orderTaken = await _categoryRepository.Query()
                .AnyAsync(x => x.Order == dto.Order && x.Id != id, cancellationToken);

            if (orderTaken)
            {
                return ServiceResult<CategoryDetailDto>.Failure(
                    "order",
                    ErrorMessages.Format(ErrorMessages.Category.OrderExists, dto.Order),
                    ServiceErrorType.Conflict);
            }

            if (dto.Image is { Length: > 0 })
            {
                try
                {
                    category.Image = await _cloudinaryService.UploadAsync(
                        dto.Image,
                        CloudinaryFolders.Categories,
                        cancellationToken);
                }
                catch (Exception)
                {
                    return ServiceResult<CategoryDetailDto>.Failure(
                        "image",
                        ErrorMessages.Category.ImageUploadFailed);
                }
            }

            var now = DateTime.UtcNow;
            category.Order = dto.Order;
            category.UpdatedDate = now;
            category.UpdaterId = updaterId;

            UpsertCategoryTranslations(category, dto.Translations, now, updaterId);

            var requestSubIds = dto.SubCategories
                .Where(x => x.Id.HasValue)
                .Select(x => x.Id!.Value)
                .ToHashSet();

            foreach (var existingSub in category.SubCategories.ToList())
            {
                if (requestSubIds.Contains(existingSub.Id))
                {
                    continue;
                }

                SoftDeleteSubCategory(existingSub, updaterId);
            }

            foreach (var subDto in dto.SubCategories)
            {
                if (subDto.Id is null)
                {
                    category.SubCategories.Add(MapSubCategory(subDto.Translations, now, updaterId));
                    continue;
                }

                var existingSub = category.SubCategories.FirstOrDefault(x => x.Id == subDto.Id.Value);
                if (existingSub is null)
                {
                    return ServiceResult<CategoryDetailDto>.Failure(
                        "subCategories",
                        ErrorMessages.Category.SubCategoryNotFound,
                        ServiceErrorType.Validation);
                }

                existingSub.UpdatedDate = now;
                existingSub.UpdaterId = updaterId;
                UpsertSubCategoryTranslations(existingSub, subDto.Translations, now, updaterId);
            }

            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var refreshed = await GetTrackedCategoryAsync(id, cancellationToken);
            return ServiceResult<CategoryDetailDto>.Success(_mapper.Map<CategoryDetailDto>(refreshed!));
        }

        public async Task<ServiceResult<CategoryDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.Query()
                .AsNoTracking()
                .Include(x => x.Translations)
                .Include(x => x.SubCategories)
                    .ThenInclude(x => x.Translations)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (category is null)
            {
                return ServiceResult<CategoryDetailDto>.NotFound("id", ErrorMessages.Category.NotFound);
            }

            return ServiceResult<CategoryDetailDto>.Success(_mapper.Map<CategoryDetailDto>(category));
        }

        public async Task<ServiceResult<PaginationResponseDto<CategoryDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<PaginationRequestDto, PaginationResponseDto<CategoryDetailDto>>(
                _paginationValidator,
                request,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var query = _categoryRepository.Query()
                .AsNoTracking()
                .Include(x => x.Translations)
                .Include(x => x.SubCategories)
                    .ThenInclude(x => x.Translations)
                .AsQueryable();

            var search = request.NormalizedSearch;
            if (search is not null)
            {
                query = query.Where(c =>
                    c.Translations.Any(t =>
                        t.Deleted == 0 && t.Name.ToLower().Contains(search.ToLower())));
            }

            query = ApplyCategorySort(query, request);

            var totalCount = await query.CountAsync(cancellationToken);

            var categories = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            IReadOnlyList<CategoryDetailDto> items = _mapper.Map<List<CategoryDetailDto>>(categories);

            return ServiceResult<PaginationResponseDto<CategoryDetailDto>>.Success(
                PaginationResponseDto<CategoryDetailDto>.Create(
                    items,
                    request.Page,
                    request.PageSize,
                    totalCount));
        }

        private static IQueryable<Category> ApplyCategorySort(
            IQueryable<Category> query,
            PaginationRequestDto request)
        {
            var desc = request.IsDescending;

            return request.NormalizedSortBy switch
            {
                "id" => desc
                    ? query.OrderByDescending(c => c.Id)
                    : query.OrderBy(c => c.Id),
                "az" => desc
                    ? query.OrderByDescending(c => c.Translations
                        .Where(t => t.LanguageCode == "az" && t.Deleted == 0)
                        .Select(t => t.Name)
                        .FirstOrDefault())
                    : query.OrderBy(c => c.Translations
                        .Where(t => t.LanguageCode == "az" && t.Deleted == 0)
                        .Select(t => t.Name)
                        .FirstOrDefault()),
                "en" => desc
                    ? query.OrderByDescending(c => c.Translations
                        .Where(t => t.LanguageCode == "en" && t.Deleted == 0)
                        .Select(t => t.Name)
                        .FirstOrDefault())
                    : query.OrderBy(c => c.Translations
                        .Where(t => t.LanguageCode == "en" && t.Deleted == 0)
                        .Select(t => t.Name)
                        .FirstOrDefault()),
                "ru" => desc
                    ? query.OrderByDescending(c => c.Translations
                        .Where(t => t.LanguageCode == "ru" && t.Deleted == 0)
                        .Select(t => t.Name)
                        .FirstOrDefault())
                    : query.OrderBy(c => c.Translations
                        .Where(t => t.LanguageCode == "ru" && t.Deleted == 0)
                        .Select(t => t.Name)
                        .FirstOrDefault()),
                _ => desc
                    ? query.OrderByDescending(c => c.Order)
                    : query.OrderBy(c => c.Order),
            };
        }

        public async Task<ServiceResult<bool>> DeleteAsync(
            int id,
            int? deletorId = null,
            CancellationToken cancellationToken = default)
        {
            var category = await GetTrackedCategoryAsync(id, cancellationToken);
            if (category is null)
            {
                return ServiceResult<bool>.NotFound("id", ErrorMessages.Category.NotFound);
            }

            foreach (var subCategory in category.SubCategories.Where(s => s.Deleted == 0).ToList())
            {
                SoftDeleteSubCategory(subCategory, deletorId);
            }

            foreach (var translation in category.Translations.Where(t => t.Deleted == 0).ToList())
            {
                _categoryTranslationRepository.SoftDelete(translation, deletorId);
            }

            _categoryRepository.SoftDelete(category, deletorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<bool>.Success(true);
        }

        private async Task<Category?> GetTrackedCategoryAsync(int id, CancellationToken cancellationToken)
        {
            return await _categoryRepository.Query()
                .Include(x => x.Translations)
                .Include(x => x.SubCategories)
                    .ThenInclude(x => x.Translations)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        private void UpsertCategoryTranslations(
            Category category,
            List<TranslationInputDto> translations,
            DateTime now,
            int? userId)
        {
            var incomingCodes = translations
                .Select(t => t.LanguageCode.Trim().ToLowerInvariant())
                .ToHashSet();

            foreach (var existing in category.Translations.ToList())
            {
                if (!incomingCodes.Contains(existing.LanguageCode))
                {
                    _categoryTranslationRepository.SoftDelete(existing, userId);
                }
            }

            foreach (var translation in translations)
            {
                var code = translation.LanguageCode.Trim().ToLowerInvariant();
                var name = translation.Name.Trim();
                var existing = category.Translations.FirstOrDefault(x => x.LanguageCode == code);

                if (existing is null)
                {
                    category.Translations.Add(MapCategoryTranslation(translation, now, userId));
                    continue;
                }

                existing.Name = name;
                existing.UpdatedDate = now;
                existing.UpdaterId = userId;
            }
        }

        private void UpsertSubCategoryTranslations(
            SubCategory subCategory,
            List<TranslationInputDto> translations,
            DateTime now,
            int? userId)
        {
            var incomingCodes = translations
                .Select(t => t.LanguageCode.Trim().ToLowerInvariant())
                .ToHashSet();

            foreach (var existing in subCategory.Translations.ToList())
            {
                if (!incomingCodes.Contains(existing.LanguageCode))
                {
                    _subCategoryTranslationRepository.SoftDelete(existing, userId);
                }
            }

            foreach (var translation in translations)
            {
                var code = translation.LanguageCode.Trim().ToLowerInvariant();
                var name = translation.Name.Trim();
                var existing = subCategory.Translations.FirstOrDefault(x => x.LanguageCode == code);

                if (existing is null)
                {
                    subCategory.Translations.Add(MapSubCategoryTranslation(translation, now, userId));
                    continue;
                }

                existing.Name = name;
                existing.UpdatedDate = now;
                existing.UpdaterId = userId;
            }
        }

        private void SoftDeleteSubCategory(SubCategory subCategory, int? deletorId)
        {
            foreach (var translation in subCategory.Translations.ToList())
            {
                _subCategoryTranslationRepository.SoftDelete(translation, deletorId);
            }

            _subCategoryRepository.SoftDelete(subCategory, deletorId);
        }

        private CategoryTranslation MapCategoryTranslation(
            TranslationInputDto translation,
            DateTime now,
            int? creatorId)
        {
            var entity = _mapper.Map<CategoryTranslation>(translation);
            entity.CreateDate = now;
            entity.CreatorId = creatorId;
            entity.Deleted = 0;
            return entity;
        }

        private SubCategory MapSubCategory(
            List<TranslationInputDto> translations,
            DateTime now,
            int? creatorId)
        {
            var subCategory = new SubCategory
            {
                CreateDate = now,
                CreatorId = creatorId,
                Deleted = 0,
            };

            foreach (var translation in translations)
            {
                subCategory.Translations.Add(MapSubCategoryTranslation(translation, now, creatorId));
            }

            return subCategory;
        }

        private SubCategoryTranslation MapSubCategoryTranslation(
            TranslationInputDto translation,
            DateTime now,
            int? creatorId)
        {
            var entity = _mapper.Map<SubCategoryTranslation>(translation);
            entity.CreateDate = now;
            entity.CreatorId = creatorId;
            entity.Deleted = 0;
            return entity;
        }
    }
}
