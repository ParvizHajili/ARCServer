using ARCServer.Business.Common.Messages;
using ARCServer.Business.Common.Validation;
using ARCServer.Business.Dtos.Brands;
using ARCServer.Business.Validators.Categories;
using FluentValidation;

namespace ARCServer.Business.Validators.Brands
{
    public class CreateBrandDtoValidator : AbstractValidator<CreateBrandDto>
    {
        public CreateBrandDtoValidator()
        {
            RuleFor(x => x.Translations)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.Translations))
                .Must(TranslationRules.HaveRequiredLanguages)
                .WithMessage(ErrorMessages.Brand.TranslationsRequired);

            RuleFor(x => x.Translations)
                .Must(TranslationRules.HaveUniqueLanguageCodes)
                .WithMessage(ErrorMessages.Brand.TranslationsUnique);

            RuleForEach(x => x.Translations)
                .SetValidator(new TranslationInputDtoValidator());
        }
    }

    public class UpdateBrandDtoValidator : AbstractValidator<UpdateBrandDto>
    {
        public UpdateBrandDtoValidator()
        {
            RuleFor(x => x.Translations)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.Translations))
                .Must(TranslationRules.HaveRequiredLanguages)
                .WithMessage(ErrorMessages.Brand.TranslationsRequired);

            RuleFor(x => x.Translations)
                .Must(TranslationRules.HaveUniqueLanguageCodes)
                .WithMessage(ErrorMessages.Brand.TranslationsUnique);

            RuleForEach(x => x.Translations)
                .SetValidator(new TranslationInputDtoValidator());
        }
    }
}
