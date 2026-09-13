using ARCServer.Business.Common.Messages;
using ARCServer.Business.Common.Validation;
using ARCServer.Business.Dtos.ManufacturerCountries;
using ARCServer.Business.Validators.Categories;
using FluentValidation;

namespace ARCServer.Business.Validators.ManufacturerCountries
{
    public class CreateManufacturerCountryDtoValidator : AbstractValidator<CreateManufacturerCountryDto>
    {
        public CreateManufacturerCountryDtoValidator()
        {
            RuleFor(x => x.Translations)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.Translations))
                .Must(TranslationRules.HaveRequiredLanguages)
                .WithMessage(ErrorMessages.ManufacturerCountry.TranslationsRequired);

            RuleFor(x => x.Translations)
                .Must(TranslationRules.HaveUniqueLanguageCodes)
                .WithMessage(ErrorMessages.ManufacturerCountry.TranslationsUnique);

            RuleForEach(x => x.Translations)
                .SetValidator(new TranslationInputDtoValidator());
        }
    }

    public class UpdateManufacturerCountryDtoValidator : AbstractValidator<UpdateManufacturerCountryDto>
    {
        public UpdateManufacturerCountryDtoValidator()
        {
            RuleFor(x => x.Translations)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.Translations))
                .Must(TranslationRules.HaveRequiredLanguages)
                .WithMessage(ErrorMessages.ManufacturerCountry.TranslationsRequired);

            RuleFor(x => x.Translations)
                .Must(TranslationRules.HaveUniqueLanguageCodes)
                .WithMessage(ErrorMessages.ManufacturerCountry.TranslationsUnique);

            RuleForEach(x => x.Translations)
                .SetValidator(new TranslationInputDtoValidator());
        }
    }
}
