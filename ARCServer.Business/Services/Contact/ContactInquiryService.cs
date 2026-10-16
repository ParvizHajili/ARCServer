using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Common;
using ARCServer.Business.Dtos.Contact;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Contact
{
    public interface IContactInquiryService
    {
        Task<ServiceResult<ContactInquiryDto>> CreateAsync(
            CreateContactInquiryDto dto,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PaginationResponseDto<ContactInquiryDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default);
    }

    public class ContactInquiryService : BaseService, IContactInquiryService
    {
        private readonly IRepository<ContactInquiry> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateContactInquiryDto> _createValidator;
        private readonly IValidator<PaginationRequestDto> _paginationValidator;

        public ContactInquiryService(
            IRepository<ContactInquiry> repository,
            IUnitOfWork unitOfWork,
            IValidator<CreateContactInquiryDto> createValidator,
            IValidator<PaginationRequestDto> paginationValidator)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _paginationValidator = paginationValidator;
        }

        public async Task<ServiceResult<ContactInquiryDto>> CreateAsync(
            CreateContactInquiryDto dto,
            CancellationToken cancellationToken = default)
        {
            dto.Name = dto.Name?.Trim() ?? string.Empty;
            dto.Message = dto.Message?.Trim() ?? string.Empty;
            dto.Phone = NormalizePhone(dto.Phone);
            dto.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();

            var validationFailure = await ValidateAsync<CreateContactInquiryDto, ContactInquiryDto>(
                _createValidator,
                dto,
                cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var inquiry = new ContactInquiry
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                Message = dto.Message,
                CreateDate = DateTime.UtcNow,
                Deleted = 0,
            };

            await _repository.AddAsync(inquiry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<ContactInquiryDto>.Success(Map(inquiry));
        }

        public async Task<ServiceResult<PaginationResponseDto<ContactInquiryDto>>> GetPagedAsync(
            PaginationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var validationFailure =
                await ValidateAsync<PaginationRequestDto, PaginationResponseDto<ContactInquiryDto>>(
                    _paginationValidator,
                    request,
                    cancellationToken);
            if (validationFailure is not null)
            {
                return validationFailure;
            }

            var query = _repository.Query().AsNoTracking();
            var search = request.NormalizedSearch;
            if (search is not null)
            {
                var pattern = $"%{EscapeLike(search)}%";
                query = query.Where(x =>
                    EF.Functions.Like(x.Name, pattern)
                    || (x.Phone != null && EF.Functions.Like(x.Phone, pattern))
                    || (x.Email != null && EF.Functions.Like(x.Email, pattern))
                    || EF.Functions.Like(x.Message, pattern));
            }

            var desc = request.IsDescending;
            query = request.NormalizedSortBy switch
            {
                "name" => desc
                    ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id)
                    : query.OrderBy(x => x.Name).ThenByDescending(x => x.Id),
                "id" => desc
                    ? query.OrderByDescending(x => x.Id)
                    : query.OrderBy(x => x.Id),
                _ => desc
                    ? query.OrderByDescending(x => x.CreateDate).ThenByDescending(x => x.Id)
                    : query.OrderBy(x => x.CreateDate).ThenBy(x => x.Id),
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .Select(x => new ContactInquiryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Phone = x.Phone,
                    Email = x.Email,
                    Message = x.Message,
                    CreateDate = x.CreateDate,
                })
                .ToListAsync(cancellationToken);

            return ServiceResult<PaginationResponseDto<ContactInquiryDto>>.Success(
                PaginationResponseDto<ContactInquiryDto>.Create(
                    items,
                    request.Page,
                    request.PageSize,
                    totalCount));
        }

        private static ContactInquiryDto Map(ContactInquiry inquiry) =>
            new()
            {
                Id = inquiry.Id,
                Name = inquiry.Name,
                Phone = inquiry.Phone,
                Email = inquiry.Email,
                Message = inquiry.Message,
                CreateDate = inquiry.CreateDate,
            };

        private static string? NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            var trimmed = phone.Trim();
            if (trimmed is "+994" or "+994 ")
            {
                return null;
            }

            return trimmed;
        }

        private static string EscapeLike(string value) =>
            value
                .Replace("[", "[[]", StringComparison.Ordinal)
                .Replace("%", "[%]", StringComparison.Ordinal)
                .Replace("_", "[_]", StringComparison.Ordinal);
    }
}
