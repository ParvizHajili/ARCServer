using ARCServer.Business.Common.Messages;
using ARCServer.Business.Common.Validation;
using ARCServer.Business.Dtos.Categories;
using FluentValidation;

namespace ARCServer.Business.Validators.Categories
{
    public class TranslationInputDtoValidator : AbstractValidator<TranslationInputDto>
    {
        public TranslationInputDtoValidator()
        {
            RuleFor(x => x.LanguageCode)
                .NotEmpty()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.NotEmpty, ErrorMessages.Fields.LanguageCode))
                .MaximumLength(10)
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.MaxLength, ErrorMessages.Fields.LanguageCode, 10))
                .Must(TranslationRules.IsAllowedLanguage)
                .WithMessage(ErrorMessages.Category.InvalidLanguageCode);

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.NotEmpty, ErrorMessages.Fields.Name))
                .MaximumLength(200)
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.MaxLength, ErrorMessages.Fields.Name, 200));
        }
    }

    public class CreateSubCategoryDtoValidator : AbstractValidator<CreateSubCategoryDto>
    {
        public CreateSubCategoryDtoValidator()
        {
            RuleFor(x => x.Translations)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.Translations))
                .Must(TranslationRules.HaveRequiredLanguages)
                .WithMessage(ErrorMessages.Category.SubTranslationsRequired);

            RuleFor(x => x.Translations)
                .Must(TranslationRules.HaveUniqueLanguageCodes)
                .WithMessage(ErrorMessages.Category.SubTranslationsUnique);

            RuleForEach(x => x.Translations)
                .SetValidator(new TranslationInputDtoValidator());
        }
    }

    public class UpdateSubCategoryDtoValidator : AbstractValidator<UpdateSubCategoryDto>
    {
        public UpdateSubCategoryDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .When(x => x.Id.HasValue)
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.GreaterThan, ErrorMessages.Fields.Id, 0));

            RuleFor(x => x.Translations)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.Translations))
                .Must(TranslationRules.HaveRequiredLanguages)
                .WithMessage(ErrorMessages.Category.SubTranslationsRequired);

            RuleFor(x => x.Translations)
                .Must(TranslationRules.HaveUniqueLanguageCodes)
                .WithMessage(ErrorMessages.Category.SubTranslationsUnique);

            RuleForEach(x => x.Translations)
                .SetValidator(new TranslationInputDtoValidator());
        }
    }

    public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
    {
        public CreateCategoryDtoValidator()
        {
            RuleFor(x => x.Image)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.Image))
                .Must(file => file is { Length: > 0 })
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.NotEmpty, ErrorMessages.Fields.Image));

            RuleFor(x => x.Order)
                .GreaterThan(0)
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.GreaterThan, ErrorMessages.Fields.Order, 0));

            RuleFor(x => x.Translations)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.Translations))
                .Must(TranslationRules.HaveRequiredLanguages)
                .WithMessage(ErrorMessages.Category.TranslationsRequired);

            RuleFor(x => x.Translations)
                .Must(TranslationRules.HaveUniqueLanguageCodes)
                .WithMessage(ErrorMessages.Category.TranslationsUnique);

            RuleForEach(x => x.Translations)
                .SetValidator(new TranslationInputDtoValidator());

            RuleFor(x => x.SubCategories)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.SubCategories));

            RuleForEach(x => x.SubCategories)
                .SetValidator(new CreateSubCategoryDtoValidator());
        }
    }

    public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
    {
        public UpdateCategoryDtoValidator()
        {
            When(x => x.Image is not null, () =>
            {
                RuleFor(x => x.Image!)
                    .Must(file => file.Length > 0)
                    .WithMessage(ErrorMessages.Format(ErrorMessages.Common.NotEmpty, ErrorMessages.Fields.Image));
            });

            RuleFor(x => x.Order)
                .GreaterThan(0)
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.GreaterThan, ErrorMessages.Fields.Order, 0));

            RuleFor(x => x.Translations)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.Translations))
                .Must(TranslationRules.HaveRequiredLanguages)
                .WithMessage(ErrorMessages.Category.TranslationsRequired);

            RuleFor(x => x.Translations)
                .Must(TranslationRules.HaveUniqueLanguageCodes)
                .WithMessage(ErrorMessages.Category.TranslationsUnique);

            RuleForEach(x => x.Translations)
                .SetValidator(new TranslationInputDtoValidator());

            RuleFor(x => x.SubCategories)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.SubCategories));

            RuleFor(x => x.SubCategories)
                .Must(HaveUniqueSubCategoryIds)
                .WithMessage(ErrorMessages.Category.SubCategoryIdsUnique);

            RuleForEach(x => x.SubCategories)
                .SetValidator(new UpdateSubCategoryDtoValidator());
        }

        private static bool HaveUniqueSubCategoryIds(List<UpdateSubCategoryDto> subCategories)
        {
            var ids = subCategories
                .Where(x => x.Id.HasValue)
                .Select(x => x.Id!.Value)
                .ToList();

            return ids.Count == ids.Distinct().Count();
        }
    }
}
