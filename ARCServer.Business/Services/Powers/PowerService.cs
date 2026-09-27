using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.Powers;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Powers
{
    public class PowerService : BaseService, IPowerService
    {
        private readonly IRepository<Power> _powerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreatePowerDto> _createValidator;
        private readonly IValidator<UpdatePowerDto> _updateValidator;
        private readonly IValidator<PaginationRequestDto> _paginationValidator;

        public PowerService(
            IRepository<Power> powerRepository,
            IUnitOfWork unitOfWork,
            IValidator<CreatePowerDto> createValidator,
            IValidator<UpdatePowerDto> updateValidator,
            IValidator<PaginationRequestDto> paginationValidator)
        {
            _powerRepository = powerRepository;
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _paginationValidator = paginationValidator;
        }

        public async Task<ServiceResult<PowerDetailDto>> CreateAsync(
            CreatePowerDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<CreatePowerDto, PowerDetailDto>(
                _createValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            if (await ValueExistsAsync(dto.Value, null, cancellationToken))
            {
                return ServiceResult<PowerDetailDto>.Failure(
                    "value",
                    ErrorMessages.Format(ErrorMessages.Power.ValueExists, dto.Value),
                    ServiceErrorType.Conflict);
            }

            var power = new Power
            {
                Value = dto.Value,
                CreateDate = DateTime.UtcNow,
                CreatorId = creatorId,
                Deleted = 0,
            };

            await _powerRepository.AddAsync(power, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<PowerDetailDto>.Success(Map(power));
        }

        public async Task<ServiceResult<PowerDetailDto>> UpdateAsync(
            int id,
            UpdatePowerDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<UpdatePowerDto, PowerDetailDto>(
                _updateValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var power = await _powerRepository.Query()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (power is null)
            {
                return ServiceResult<PowerDetailDto>.NotFound("id", ErrorMessages.Power.NotFound);
            }

            if (await ValueExistsAsync(dto.Value, id, cancellationToken))
            {
                return ServiceResult<PowerDetailDto>.Failure(
                    "value",
                    ErrorMessages.Format(ErrorMessages.Power.ValueExists, dto.Value),
                    ServiceErrorType.Conflict);
            }

            power.Value = dto.Value;
            power.UpdatedDate = DateTime.UtcNow;
            power.UpdaterId = updaterId;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<PowerDetailDto>.Success(Map(power));
        }

        public async Task<ServiceResult<PowerDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var power = await _powerRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (power is null)
            {
                return ServiceResult<PowerDetailDto>.NotFound("id", ErrorMessages.Power.NotFound);
            }

            return ServiceResult<PowerDetailDto>.Success(Map(power));
        }

        public async Task<ServiceResult<PaginationResponseDto<PowerDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var validationFailure =
                await ValidateAsync<PaginationRequestDto, PaginationResponseDto<PowerDetailDto>>(
                    _paginationValidator,
                    request,
                    cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var query = _powerRepository.Query().AsNoTracking();
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
                .Select(x => new PowerDetailDto { Id = x.Id, Value = x.Value })
                .ToListAsync(cancellationToken);

            return ServiceResult<PaginationResponseDto<PowerDetailDto>>.Success(
                PaginationResponseDto<PowerDetailDto>.Create(
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
            var power = await _powerRepository.Query()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (power is null)
            {
                return ServiceResult<bool>.NotFound("id", ErrorMessages.Power.NotFound);
            }

            _powerRepository.SoftDelete(power, deletorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult<bool>.Success(true);
        }

        private async Task<bool> ValueExistsAsync(
            decimal value,
            int? excludeId,
            CancellationToken cancellationToken)
        {
            var query = _powerRepository.Query().Where(x => x.Value == value);
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        private static PowerDetailDto Map(Power power) =>
            new() { Id = power.Id, Value = power.Value };
    }
}
