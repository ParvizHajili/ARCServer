using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Brands;
using ARCServer.Business.Dtos.Categories;
using ARCServer.Business.Dtos.Common;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Brands
{
    public class BrandService : BaseService, IBrandService
    {
        private readonly IRepository<Brand> _brandRepository;
        private readonly IRepository<BrandTranslation> _translationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateBrandDto> _createValidator;
        private readonly IValidator<UpdateBrandDto> _updateValidator;
        private readonly IValidator<PaginationRequestDto> _paginationValidator;

        public BrandService(
            IRepository<Brand> brandRepository,
            IRepository<BrandTranslation> translationRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateBrandDto> createValidator,
            IValidator<UpdateBrandDto> updateValidator,
            IValidator<PaginationRequestDto> paginationValidator)
        {
            _brandRepository = brandRepository;
            _translationRepository = translationRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _paginationValidator = paginationValidator;
        }

        public async Task<ServiceResult<BrandDetailDto>> CreateAsync(
            CreateBrandDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<CreateBrandDto, BrandDetailDto>(
                _createValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var nameConflict = await FindNameConflictAsync(dto.Translations, excludeBrandId: null, cancellationToken);
            if (nameConflict is not null)
            {
                return nameConflict;
            }

            var now = DateTime.UtcNow;
            var brand = new Brand
            {
                CreateDate = now,
                CreatorId = creatorId,
                Deleted = 0,
            };

            foreach (var translation in dto.Translations)
            {
                brand.Translations.Add(MapTranslation(translation, now, creatorId));
            }

            await _brandRepository.AddAsync(brand, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<BrandDetailDto>.Success(_mapper.Map<BrandDetailDto>(brand));
        }

        public async Task<ServiceResult<BrandDetailDto>> UpdateAsync(
            int id,
            UpdateBrandDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<UpdateBrandDto, BrandDetailDto>(
                _updateValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var brand = await GetTrackedAsync(id, cancellationToken);
            if (brand is null)
            {
                return ServiceResult<BrandDetailDto>.NotFound("id", ErrorMessages.Brand.NotFound);
            }

            var nameConflict = await FindNameConflictAsync(dto.Translations, excludeBrandId: id, cancellationToken);
            if (nameConflict is not null)
            {
                return nameConflict;
            }

            var now = DateTime.UtcNow;
            UpsertTranslations(brand, dto.Translations, now, updaterId);
            brand.UpdatedDate = now;
            brand.UpdaterId = updaterId;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var refreshed = await GetTrackedAsync(id, cancellationToken);
            return ServiceResult<BrandDetailDto>.Success(_mapper.Map<BrandDetailDto>(refreshed!));
        }

        public async Task<ServiceResult<BrandDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.Query()
                .AsNoTracking()
                .Include(x => x.Translations)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (brand is null)
            {
                return ServiceResult<BrandDetailDto>.NotFound("id", ErrorMessages.Brand.NotFound);
            }

            return ServiceResult<BrandDetailDto>.Success(_mapper.Map<BrandDetailDto>(brand));
        }

        public async Task<ServiceResult<PaginationResponseDto<BrandDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var validationFailure =
                await ValidateAsync<PaginationRequestDto, PaginationResponseDto<BrandDetailDto>>(
                    _paginationValidator,
                    request,
                    cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var query = _brandRepository.Query()
                .AsNoTracking()
                .Include(x => x.Translations)
                .AsQueryable();

            var search = request.NormalizedSearch;
            if (search is not null)
            {
                query = query.Where(c =>
                    c.Translations.Any(t =>
                        t.Deleted == 0 && t.Name.ToLower().Contains(search.ToLower())));
            }

            query = ApplySort(query, request);

            var totalCount = await query.CountAsync(cancellationToken);
            var brands = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            IReadOnlyList<BrandDetailDto> items = _mapper.Map<List<BrandDetailDto>>(brands);

            return ServiceResult<PaginationResponseDto<BrandDetailDto>>.Success(
                PaginationResponseDto<BrandDetailDto>.Create(
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
            var brand = await GetTrackedAsync(id, cancellationToken);
            if (brand is null)
            {
                return ServiceResult<bool>.NotFound("id", ErrorMessages.Brand.NotFound);
            }

            foreach (var translation in brand.Translations.Where(t => t.Deleted == 0).ToList())
            {
                _translationRepository.SoftDelete(translation, deletorId);
            }

            _brandRepository.SoftDelete(brand, deletorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<bool>.Success(true);
        }

        private async Task<ServiceResult<BrandDetailDto>?> FindNameConflictAsync(
            List<TranslationInputDto> translations,
            int? excludeBrandId,
            CancellationToken cancellationToken)
        {
            foreach (var translation in translations)
            {
                var code = translation.LanguageCode.Trim().ToLowerInvariant();
                var name = translation.Name.Trim();

                var existsQuery = _translationRepository.Query()
                    .Where(t =>
                        t.LanguageCode == code
                        && t.Name.ToLower() == name.ToLower());

                if (excludeBrandId.HasValue)
                {
                    existsQuery = existsQuery.Where(t => t.BrandId != excludeBrandId.Value);
                }

                if (await existsQuery.AnyAsync(cancellationToken))
                {
                    return ServiceResult<BrandDetailDto>.Failure(
                        "translations",
                        ErrorMessages.Format(ErrorMessages.Brand.NameExists, name),
                        ServiceErrorType.Conflict);
                }
            }

            return null;
        }

        private static IQueryable<Brand> ApplySort(
            IQueryable<Brand> query,
            PaginationRequestDto request)
        {
            var desc = request.IsDescending;

            return request.NormalizedSortBy switch
            {
                "id" => desc
                    ? query.OrderByDescending(c => c.Id)
                    : query.OrderBy(c => c.Id),
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
                    ? query.OrderByDescending(c => c.Translations
                        .Where(t => t.LanguageCode == "az" && t.Deleted == 0)
                        .Select(t => t.Name)
                        .FirstOrDefault())
                    : query.OrderBy(c => c.Translations
                        .Where(t => t.LanguageCode == "az" && t.Deleted == 0)
                        .Select(t => t.Name)
                        .FirstOrDefault()),
            };
        }

        private async Task<Brand?> GetTrackedAsync(int id, CancellationToken cancellationToken)
        {
            return await _brandRepository.Query()
                .Include(x => x.Translations)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        private void UpsertTranslations(
            Brand brand,
            List<TranslationInputDto> translations,
            DateTime now,
            int? userId)
        {
            var incomingCodes = translations
                .Select(t => t.LanguageCode.Trim().ToLowerInvariant())
                .ToHashSet();

            foreach (var existing in brand.Translations.ToList())
            {
                if (!incomingCodes.Contains(existing.LanguageCode))
                {
                    _translationRepository.SoftDelete(existing, userId);
                }
            }

            foreach (var translation in translations)
            {
                var code = translation.LanguageCode.Trim().ToLowerInvariant();
                var name = translation.Name.Trim();
                var existing = brand.Translations.FirstOrDefault(x => x.LanguageCode == code);

                if (existing is null)
                {
                    brand.Translations.Add(MapTranslation(translation, now, userId));
                    continue;
                }

                existing.Name = name;
                existing.UpdatedDate = now;
                existing.UpdaterId = userId;
            }
        }

        private BrandTranslation MapTranslation(
            TranslationInputDto translation,
            DateTime now,
            int? creatorId)
        {
            var entity = _mapper.Map<BrandTranslation>(translation);
            entity.CreateDate = now;
            entity.CreatorId = creatorId;
            entity.Deleted = 0;
            return entity;
        }
    }
}
