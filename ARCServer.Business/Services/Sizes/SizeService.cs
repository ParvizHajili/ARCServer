using ARCServer.Business.Common;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.Sizes;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Sizes
{
    public class SizeService : BaseService, ISizeService
    {
        private readonly IRepository<Size> _sizeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateSizeDto> _createValidator;
        private readonly IValidator<UpdateSizeDto> _updateValidator;
        private readonly IValidator<PaginationRequestDto> _paginationValidator;

        public SizeService(
            IRepository<Size> sizeRepository,
            IUnitOfWork unitOfWork,
            IValidator<CreateSizeDto> createValidator,
            IValidator<UpdateSizeDto> updateValidator,
            IValidator<PaginationRequestDto> paginationValidator)
        {
            _sizeRepository = sizeRepository;
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _paginationValidator = paginationValidator;
        }

        public async Task<ServiceResult<SizeDetailDto>> CreateAsync(
            CreateSizeDto dto,
            int? creatorId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<CreateSizeDto, SizeDetailDto>(
                _createValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            if (await ValueExistsAsync(dto.Value, null, cancellationToken))
            {
                return ServiceResult<SizeDetailDto>.Failure(
                    "value",
                    ErrorMessages.Format(ErrorMessages.Size.ValueExists, dto.Value),
                    ServiceErrorType.Conflict);
            }

            var size = new Size
            {
                Value = dto.Value,
                CreateDate = DateTime.UtcNow,
                CreatorId = creatorId,
                Deleted = 0,
            };

            await _sizeRepository.AddAsync(size, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<SizeDetailDto>.Success(Map(size));
        }

        public async Task<ServiceResult<SizeDetailDto>> UpdateAsync(
            int id,
            UpdateSizeDto dto,
            int? updaterId = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = await ValidateAsync<UpdateSizeDto, SizeDetailDto>(
                _updateValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var size = await _sizeRepository.Query()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (size is null)
            {
                return ServiceResult<SizeDetailDto>.NotFound("id", ErrorMessages.Size.NotFound);
            }

            if (await ValueExistsAsync(dto.Value, id, cancellationToken))
            {
                return ServiceResult<SizeDetailDto>.Failure(
                    "value",
                    ErrorMessages.Format(ErrorMessages.Size.ValueExists, dto.Value),
                    ServiceErrorType.Conflict);
            }

            size.Value = dto.Value;
            size.UpdatedDate = DateTime.UtcNow;
            size.UpdaterId = updaterId;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<SizeDetailDto>.Success(Map(size));
        }

        public async Task<ServiceResult<SizeDetailDto>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var size = await _sizeRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (size is null)
            {
                return ServiceResult<SizeDetailDto>.NotFound("id", ErrorMessages.Size.NotFound);
            }

            return ServiceResult<SizeDetailDto>.Success(Map(size));
        }

        public async Task<ServiceResult<PaginationResponseDto<SizeDetailDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var validationFailure =
                await ValidateAsync<PaginationRequestDto, PaginationResponseDto<SizeDetailDto>>(
                    _paginationValidator,
                    request,
                    cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var query = _sizeRepository.Query().AsNoTracking();
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
                .Select(x => new SizeDetailDto { Id = x.Id, Value = x.Value })
                .ToListAsync(cancellationToken);

            return ServiceResult<PaginationResponseDto<SizeDetailDto>>.Success(
                PaginationResponseDto<SizeDetailDto>.Create(
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
            var size = await _sizeRepository.Query()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (size is null)
            {
                return ServiceResult<bool>.NotFound("id", ErrorMessages.Size.NotFound);
            }

            _sizeRepository.SoftDelete(size, deletorId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult<bool>.Success(true);
        }

        private async Task<bool> ValueExistsAsync(
            decimal value,
            int? excludeId,
            CancellationToken cancellationToken)
        {
            var query = _sizeRepository.Query().Where(x => x.Value == value);
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        private static SizeDetailDto Map(Size size) =>
            new() { Id = size.Id, Value = size.Value };
    }
}
