using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Common;
using FluentValidation;

namespace ARCServer.Business.Validators.Common
{
    public class PaginationRequestDtoValidator : AbstractValidator<PaginationRequestDto>
    {
        public PaginationRequestDtoValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.GreaterThan, "Səhifə", 0));

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, PaginationRequestDto.MaxPageSize)
                .WithMessage(
                    $"Səhifə ölçüsü 1–{PaginationRequestDto.MaxPageSize} arasında olmalıdır.");

            RuleFor(x => x.SortDirection)
                .Must(BeValidSortDirection)
                .WithMessage("SortDirection yalnız 'asc' və ya 'desc' ola bilər.");

            RuleFor(x => x.Search)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Search))
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.MaxLength, "Axtarış", 200));

            RuleFor(x => x.SortBy)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.MaxLength, "SortBy", 50));
        }

        private static bool BeValidSortDirection(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            return value.Equals("asc", StringComparison.OrdinalIgnoreCase)
                || value.Equals("desc", StringComparison.OrdinalIgnoreCase);
        }
    }
}
