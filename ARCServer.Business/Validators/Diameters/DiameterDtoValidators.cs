using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Diameters;
using FluentValidation;

namespace ARCServer.Business.Validators.Diameters
{
    public class CreateDiameterDtoValidator : AbstractValidator<CreateDiameterDto>
    {
        public CreateDiameterDtoValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Common.GreaterThan,
                    ErrorMessages.Fields.Diameter,
                    0));
        }
    }

    public class UpdateDiameterDtoValidator : AbstractValidator<UpdateDiameterDto>
    {
        public UpdateDiameterDtoValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Common.GreaterThan,
                    ErrorMessages.Fields.Diameter,
                    0));
        }
    }
}
