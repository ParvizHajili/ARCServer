using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Powers;
using FluentValidation;

namespace ARCServer.Business.Validators.Powers
{
    public class CreatePowerDtoValidator : AbstractValidator<CreatePowerDto>
    {
        public CreatePowerDtoValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Common.GreaterThan,
                    ErrorMessages.Fields.Power,
                    0));
        }
    }

    public class UpdatePowerDtoValidator : AbstractValidator<UpdatePowerDto>
    {
        public UpdatePowerDtoValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Common.GreaterThan,
                    ErrorMessages.Fields.Power,
                    0));
        }
    }
}
