using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Categories;
using ARCServer.Business.Dtos.Colors;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Validators.Colors;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Colors
{
    public class ColorService : BaseService, IColorService
    {
        private readonly IRepository<Color> _colorRepository;
        private readonly IRepository<ColorTranslation> _translationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateColorDto> _createValidator;
        private readonly IValidator<UpdateColorDto> _updateValidator;
        private readonly IValidator<PaginationRequestDto> _paginationValidator;

        public ColorService(
            IRepository<Color> colorRepository,
            IRepository<ColorTranslation> translationRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateColorDto> createValidator,
            IValidator<UpdateColorDto> updateValidator,
            IValidator<PaginationRequestDto> paginationValidator)
        {
            _colorRepository = colorRepository;
            _translationRepository = translationRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _paginationValidator = paginationValidator;
        }

        public async Task<ServiceResult<ColorDetailDto>> CreateAsync(
            CreateColorDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<CreateColorDto, ColorDetailDto>(
                _createValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var hex = ColorHexRules.Normalize(dto.HexCode);

            var hexConflict = await FindHexConflictAsync(hex, excludeColorId: null, cancellationToken);
            if (hexConflict is not null)
            {
                return hexConflict;
            }

            var nameConflict = await FindNameConflictAsync(dto.Translations, excludeColorId: null, cancellationToken);
            if (nameConflict is not null)
            {
                return nameConflict;
            }

            var now = DateTime.UtcNow;
            var color = new Color
            {
                HexCode = hex,
                CreateDate = now,
                CreatorId = creatorId,
                Deleted = 0,
            };

            foreach (var translation in dto.Translations)
            {
                color.Translations.Add(MapTranslation(translation, now, creatorId));
            }

            await _colorRepository.AddAsync(color, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<ColorDetailDto>.Success(_mapper.Map<ColorDetailDto>(color));
        }

        public async Task<ServiceResult<ColorDetailDto>> UpdateAsync(
            int id,
            UpdateColorDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<UpdateColorDto, ColorDetailDto>(
                _updateValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var color = await GetTrackedAsync(id, cancellationToken);
            if (color is null)
            {
                return ServiceResult<ColorDetailDto>.NotFound("id", ErrorMessages.Color.NotFound);
            }

            var hex = ColorHexRules.Normalize(dto.HexCode);

            var hexConflict = await FindHexConflictAsync(hex, excludeColorId: id, cancellationToken);
            if (hexConflict is not null)
            {
                return hexConflict;
            }

            var nameConflict = await FindNameConflictAsync(dto.Translations, excludeColorId: id, cancellationToken);
            if (nameConflict is not null)
            {
                return nameConflict;
            }

            var now = DateTime.UtcNow;
            color.HexCode = hex;
            UpsertTranslations(color, dto.Translations, now, updaterId);
            color.UpdatedDate = now;
            color.UpdaterId = updaterId;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var refreshed = await GetTrackedAsync(id, cancellationToken);
            return ServiceResult<ColorDetailDto>.Success(_mapper.Map<ColorDetailDto>(refreshed!));
        }

        public async Task<ServiceResult<ColorDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var color = await _colorRepository.Query()
                .AsNoTracking()
                .Include(x => x.Translations)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (color is null)
            {
                return ServiceResult<ColorDetailDto>.NotFound("id", ErrorMessages.Color.NotFound);
            }

            return ServiceResult<ColorDetailDto>.Success(_mapper.Map<ColorDetailDto>(color));
        }

        public async Task<ServiceResult<PaginationResponseDto<ColorDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var validationFailure =
                await ValidateAsync<PaginationRequestDto, PaginationResponseDto<ColorDetailDto>>(
                    _paginationValidator,
                    request,
                    cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var query = _colorRepository.Query()
                .AsNoTracking()
                .Include(x => x.Translations)
                .AsQueryable();

            var search = request.NormalizedSearch;
            if (search is not null)
            {
                query = query.Where(c =>
                    c.HexCode.ToLower().Contains(search.ToLower())
                    || c.Translations.Any(t =>
                        t.Deleted == 0 && t.Name.ToLower().Contains(search.ToLower())));
            }

            query = ApplySort(query, request);

            var totalCount = await query.CountAsync(cancellationToken);
            var colors = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            IReadOnlyList<ColorDetailDto> items = _mapper.Map<List<ColorDetailDto>>(colors);

            return ServiceResult<PaginationResponseDto<ColorDetailDto>>.Success(
                PaginationResponseDto<ColorDetailDto>.Create(
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
            var color = await GetTrackedAsync(id, cancellationToken);
            if (color is null)
            {
                return ServiceResult<bool>.NotFound("id", ErrorMessages.Color.NotFound);
            }

            foreach (var translation in color.Translations.Where(t => t.Deleted == 0).ToList())
            {
                _translationRepository.SoftDelete(translation, deletorId);
            }

            _colorRepository.SoftDelete(color, deletorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<bool>.Success(true);
        }

        private async Task<ServiceResult<ColorDetailDto>?> FindHexConflictAsync(
            string hex,
            int? excludeColorId,
            CancellationToken cancellationToken)
        {
            var existsQuery = _colorRepository.Query()
                .Where(c => c.HexCode == hex);

            if (excludeColorId.HasValue)
            {
                existsQuery = existsQuery.Where(c => c.Id != excludeColorId.Value);
            }

            if (await existsQuery.AnyAsync(cancellationToken))
            {
                return ServiceResult<ColorDetailDto>.Failure(
                    "hexCode",
                    ErrorMessages.Format(ErrorMessages.Color.HexExists, hex),
                    ServiceErrorType.Conflict);
            }

            return null;
        }

        private async Task<ServiceResult<ColorDetailDto>?> FindNameConflictAsync(
            List<TranslationInputDto> translations,
            int? excludeColorId,
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

                if (excludeColorId.HasValue)
                {
                    existsQuery = existsQuery.Where(t => t.ColorId != excludeColorId.Value);
                }

                if (await existsQuery.AnyAsync(cancellationToken))
                {
                    return ServiceResult<ColorDetailDto>.Failure(
                        "translations",
                        ErrorMessages.Format(ErrorMessages.Color.NameExists, name),
                        ServiceErrorType.Conflict);
                }
            }

            return null;
        }

        private static IQueryable<Color> ApplySort(
            IQueryable<Color> query,
            PaginationRequestDto request)
        {
            var desc = request.IsDescending;

            return request.NormalizedSortBy switch
            {
                "id" => desc
                    ? query.OrderByDescending(c => c.Id)
                    : query.OrderBy(c => c.Id),
                "hex" or "hexcode" => desc
                    ? query.OrderByDescending(c => c.HexCode)
                    : query.OrderBy(c => c.HexCode),
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

        private async Task<Color?> GetTrackedAsync(int id, CancellationToken cancellationToken)
        {
            return await _colorRepository.Query()
                .Include(x => x.Translations)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        private void UpsertTranslations(
            Color color,
            List<TranslationInputDto> translations,
            DateTime now,
            int? userId)
        {
            var incomingCodes = translations
                .Select(t => t.LanguageCode.Trim().ToLowerInvariant())
                .ToHashSet();

            foreach (var existing in color.Translations.ToList())
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
                var existing = color.Translations.FirstOrDefault(x => x.LanguageCode == code);

                if (existing is null)
                {
                    color.Translations.Add(MapTranslation(translation, now, userId));
                    continue;
                }

                existing.Name = name;
                existing.UpdatedDate = now;
                existing.UpdaterId = userId;
            }
        }

        private ColorTranslation MapTranslation(
            TranslationInputDto translation,
            DateTime now,
            int? creatorId)
        {
            var entity = _mapper.Map<ColorTranslation>(translation);
            entity.CreateDate = now;
            entity.CreatorId = creatorId;
            entity.Deleted = 0;
            return entity;
        }
    }
}
