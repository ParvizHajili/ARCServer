using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Categories;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.ManufacturerCountries;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.ManufacturerCountries
{
    public class ManufacturerCountryService : BaseService, IManufacturerCountryService
    {
        private readonly IRepository<ManufacturerCountry> _countryRepository;
        private readonly IRepository<ManufacturerCountryTranslation> _translationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateManufacturerCountryDto> _createValidator;
        private readonly IValidator<UpdateManufacturerCountryDto> _updateValidator;
        private readonly IValidator<PaginationRequestDto> _paginationValidator;

        public ManufacturerCountryService(
            IRepository<ManufacturerCountry> countryRepository,
            IRepository<ManufacturerCountryTranslation> translationRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateManufacturerCountryDto> createValidator,
            IValidator<UpdateManufacturerCountryDto> updateValidator,
            IValidator<PaginationRequestDto> paginationValidator)
        {
            _countryRepository = countryRepository;
            _translationRepository = translationRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _paginationValidator = paginationValidator;
        }

        public async Task<ServiceResult<ManufacturerCountryDetailDto>> CreateAsync(
            CreateManufacturerCountryDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<CreateManufacturerCountryDto, ManufacturerCountryDetailDto>(
                _createValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var nameConflict = await FindNameConflictAsync(dto.Translations, excludeCountryId: null, cancellationToken);
            if (nameConflict is not null)
            {
                return nameConflict;
            }

            var now = DateTime.UtcNow;
            var country = new ManufacturerCountry
            {
                CreateDate = now,
                CreatorId = creatorId,
                Deleted = 0,
            };

            foreach (var translation in dto.Translations)
            {
                country.Translations.Add(MapTranslation(translation, now, creatorId));
            }

            await _countryRepository.AddAsync(country, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<ManufacturerCountryDetailDto>.Success(
                _mapper.Map<ManufacturerCountryDetailDto>(country));
        }

        public async Task<ServiceResult<ManufacturerCountryDetailDto>> UpdateAsync(
            int id,
            UpdateManufacturerCountryDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<UpdateManufacturerCountryDto, ManufacturerCountryDetailDto>(
                _updateValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var country = await GetTrackedAsync(id, cancellationToken);
            if (country is null)
            {
                return ServiceResult<ManufacturerCountryDetailDto>.NotFound(
                    "id",
                    ErrorMessages.ManufacturerCountry.NotFound);
            }

            var nameConflict = await FindNameConflictAsync(dto.Translations, excludeCountryId: id, cancellationToken);
            if (nameConflict is not null)
            {
                return nameConflict;
            }

            var now = DateTime.UtcNow;
            UpsertTranslations(country, dto.Translations, now, updaterId);
            country.UpdatedDate = now;
            country.UpdaterId = updaterId;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var refreshed = await GetTrackedAsync(id, cancellationToken);
            return ServiceResult<ManufacturerCountryDetailDto>.Success(
                _mapper.Map<ManufacturerCountryDetailDto>(refreshed!));
        }

        public async Task<ServiceResult<ManufacturerCountryDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var country = await _countryRepository.Query()
                .AsNoTracking()
                .Include(x => x.Translations)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (country is null)
            {
                return ServiceResult<ManufacturerCountryDetailDto>.NotFound(
                    "id",
                    ErrorMessages.ManufacturerCountry.NotFound);
            }

            return ServiceResult<ManufacturerCountryDetailDto>.Success(
                _mapper.Map<ManufacturerCountryDetailDto>(country));
        }

        public async Task<ServiceResult<PaginationResponseDto<ManufacturerCountryDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var validationFailure =
                await ValidateAsync<PaginationRequestDto, PaginationResponseDto<ManufacturerCountryDetailDto>>(
                    _paginationValidator,
                    request,
                    cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var query = _countryRepository.Query()
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
            var countries = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            IReadOnlyList<ManufacturerCountryDetailDto> items =
                _mapper.Map<List<ManufacturerCountryDetailDto>>(countries);

            return ServiceResult<PaginationResponseDto<ManufacturerCountryDetailDto>>.Success(
                PaginationResponseDto<ManufacturerCountryDetailDto>.Create(
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
            var country = await GetTrackedAsync(id, cancellationToken);
            if (country is null)
            {
                return ServiceResult<bool>.NotFound("id", ErrorMessages.ManufacturerCountry.NotFound);
            }

            foreach (var translation in country.Translations.Where(t => t.Deleted == 0).ToList())
            {
                _translationRepository.SoftDelete(translation, deletorId);
            }

            _countryRepository.SoftDelete(country, deletorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<bool>.Success(true);
        }

        private async Task<ServiceResult<ManufacturerCountryDetailDto>?> FindNameConflictAsync(
            List<TranslationInputDto> translations,
            int? excludeCountryId,
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

                if (excludeCountryId.HasValue)
                {
                    existsQuery = existsQuery.Where(t => t.ManufacturerCountryId != excludeCountryId.Value);
                }

                if (await existsQuery.AnyAsync(cancellationToken))
                {
                    return ServiceResult<ManufacturerCountryDetailDto>.Failure(
                        "translations",
                        ErrorMessages.Format(ErrorMessages.ManufacturerCountry.NameExists, name),
                        ServiceErrorType.Conflict);
                }
            }

            return null;
        }

        private static IQueryable<ManufacturerCountry> ApplySort(
            IQueryable<ManufacturerCountry> query,
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

        private async Task<ManufacturerCountry?> GetTrackedAsync(int id, CancellationToken cancellationToken)
        {
            return await _countryRepository.Query()
                .Include(x => x.Translations)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        private void UpsertTranslations(
            ManufacturerCountry country,
            List<TranslationInputDto> translations,
            DateTime now,
            int? userId)
        {
            var incomingCodes = translations
                .Select(t => t.LanguageCode.Trim().ToLowerInvariant())
                .ToHashSet();

            foreach (var existing in country.Translations.ToList())
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
                var existing = country.Translations.FirstOrDefault(x => x.LanguageCode == code);

                if (existing is null)
                {
                    country.Translations.Add(MapTranslation(translation, now, userId));
                    continue;
                }

                existing.Name = name;
                existing.UpdatedDate = now;
                existing.UpdaterId = userId;
            }
        }

        private ManufacturerCountryTranslation MapTranslation(
            TranslationInputDto translation,
            DateTime now,
            int? creatorId)
        {
            var entity = _mapper.Map<ManufacturerCountryTranslation>(translation);
            entity.CreateDate = now;
            entity.CreatorId = creatorId;
            entity.Deleted = 0;
            return entity;
        }
    }
}
