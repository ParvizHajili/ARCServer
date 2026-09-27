using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Sizes;
using FluentValidation;

namespace ARCServer.Business.Validators.Sizes
{
    public class CreateSizeDtoValidator : AbstractValidator<CreateSizeDto>
    {
        public CreateSizeDtoValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Common.GreaterThan,
                    ErrorMessages.Fields.Size,
                    0));
        }
    }

    public class UpdateSizeDtoValidator : AbstractValidator<UpdateSizeDto>
    {
        public UpdateSizeDtoValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Common.GreaterThan,
                    ErrorMessages.Fields.Size,
                    0));
        }
    }
}
