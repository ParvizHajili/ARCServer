using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.Diameters;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Diameters
{
    public class DiameterService : BaseService, IDiameterService
    {
        private readonly IRepository<Diameter> _diameterRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateDiameterDto> _createValidator;
        private readonly IValidator<UpdateDiameterDto> _updateValidator;
        private readonly IValidator<PaginationRequestDto> _paginationValidator;

        public DiameterService(
            IRepository<Diameter> diameterRepository,
            IUnitOfWork unitOfWork,
            IValidator<CreateDiameterDto> createValidator,
            IValidator<UpdateDiameterDto> updateValidator,
            IValidator<PaginationRequestDto> paginationValidator)
        {
            _diameterRepository = diameterRepository;
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _paginationValidator = paginationValidator;
        }

        public async Task<ServiceResult<DiameterDetailDto>> CreateAsync(
            CreateDiameterDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<CreateDiameterDto, DiameterDetailDto>(
                _createValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            if (await ValueExistsAsync(dto.Value, null, cancellationToken))
            {
                return ServiceResult<DiameterDetailDto>.Failure(
                    "value",
                    ErrorMessages.Format(ErrorMessages.Diameter.ValueExists, dto.Value),
                    ServiceErrorType.Conflict);
            }

            var diameter = new Diameter
            {
                Value = dto.Value,
                CreateDate = DateTime.UtcNow,
                CreatorId = creatorId,
                Deleted = 0,
            };

            await _diameterRepository.AddAsync(diameter, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<DiameterDetailDto>.Success(Map(diameter));
        }

        public async Task<ServiceResult<DiameterDetailDto>> UpdateAsync(
            int id,
            UpdateDiameterDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<UpdateDiameterDto, DiameterDetailDto>(
                _updateValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var diameter = await _diameterRepository.Query()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (diameter is null)
            {
                return ServiceResult<DiameterDetailDto>.NotFound("id", ErrorMessages.Diameter.NotFound);
            }

            if (await ValueExistsAsync(dto.Value, id, cancellationToken))
            {
                return ServiceResult<DiameterDetailDto>.Failure(
                    "value",
                    ErrorMessages.Format(ErrorMessages.Diameter.ValueExists, dto.Value),
                    ServiceErrorType.Conflict);
            }

            diameter.Value = dto.Value;
            diameter.UpdatedDate = DateTime.UtcNow;
            diameter.UpdaterId = updaterId;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<DiameterDetailDto>.Success(Map(diameter));
        }

        public async Task<ServiceResult<DiameterDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var diameter = await _diameterRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (diameter is null)
            {
                return ServiceResult<DiameterDetailDto>.NotFound("id", ErrorMessages.Diameter.NotFound);
            }

            return ServiceResult<DiameterDetailDto>.Success(Map(diameter));
        }

        public async Task<ServiceResult<PaginationResponseDto<DiameterDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var validationFailure =
                await ValidateAsync<PaginationRequestDto, PaginationResponseDto<DiameterDetailDto>>(
                    _paginationValidator,
                    request,
                    cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var query = _diameterRepository.Query().AsNoTracking();
            var search = request.NormalizedSearch;
            if (search is not null && decimal.TryParse(search, out var number))
            {
                query = query.Where(x => x.Value == number);
            }

            var desc = request.IsDescending;
            query = request.NormalizedSortBy == "id"
                ? desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
                : desc ? query.OrderByDescending(x => x.Value) : query.OrderBy(x => x.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .Select(x => new DiameterDetailDto { Id = x.Id, Value = x.Value })
                .ToListAsync(cancellationToken);

            return ServiceResult<PaginationResponseDto<DiameterDetailDto>>.Success(
                PaginationResponseDto<DiameterDetailDto>.Create(
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
            var diameter = await _diameterRepository.Query()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (diameter is null)
            {
                return ServiceResult<bool>.NotFound("id", ErrorMessages.Diameter.NotFound);
            }

            _diameterRepository.SoftDelete(diameter, deletorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult<bool>.Success(true);
        }

        private async Task<bool> ValueExistsAsync(
            decimal value,
            int? excludeId,
            CancellationToken cancellationToken)
        {
            var query = _diameterRepository.Query().Where(x => x.Value == value);
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        private static DiameterDetailDto Map(Diameter diameter) =>
            new() { Id = diameter.Id, Value = diameter.Value };
    }
}
