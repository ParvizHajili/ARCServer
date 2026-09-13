using ARCServer.Business.Common.Messages;
using ARCServer.Business.Common.Validation;
using ARCServer.Business.Dtos.Products;
using ARCServer.Domain.Common;
using FluentValidation;

namespace ARCServer.Business.Validators.Products
{
    public class ProductTranslationInputDtoValidator : AbstractValidator<ProductTranslationInputDto>
    {
        public ProductTranslationInputDtoValidator()
        {
            RuleFor(x => x.LanguageCode)
                .NotEmpty()
                .MaximumLength(10)
                .Must(TranslationRules.IsAllowedLanguage)
                .WithMessage(ErrorMessages.Category.InvalidLanguageCode);

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.NotEmpty, ErrorMessages.Fields.Name))
                .MaximumLength(300)
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.MaxLength, ErrorMessages.Fields.Name, 300));

            RuleFor(x => x.Description)
                .MaximumLength(4000)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Common.MaxLength,
                    ErrorMessages.Fields.Description,
                    4000));

            RuleFor(x => x.Description)
                .NotEmpty()
                .When(x => x.LanguageCode.Trim().Equals("az", StringComparison.OrdinalIgnoreCase))
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Common.NotEmpty,
                    ErrorMessages.Fields.Description));
        }
    }

    public static class ProductTranslationRules
    {
        public static bool HaveRequiredLanguages(List<ProductTranslationInputDto> translations)
        {
            if (translations is null || translations.Count == 0)
            {
                return false;
            }

            var codes = translations
                .Select(t => t.LanguageCode.Trim().ToLowerInvariant())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToHashSet();

            return SupportedLanguages.RequiredCodes.All(codes.Contains);
        }

        public static bool HaveUniqueLanguageCodes(List<ProductTranslationInputDto> translations)
        {
            if (translations is null)
            {
                return true;
            }

            var codes = translations
                .Select(t => t.LanguageCode.Trim().ToLowerInvariant())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToList();

            return codes.Count == codes.Distinct().Count();
        }
    }

    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Size)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Diameter)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.PowerAmperes)
                .GreaterThan(0)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Common.GreaterThan,
                    ErrorMessages.Fields.PowerAmperes,
                    0));

            RuleFor(x => x.CategoryId)
                .GreaterThan(0);

            RuleFor(x => x.SubCategoryId)
                .GreaterThan(0)
                .When(x => x.SubCategoryId.HasValue);

            RuleFor(x => x.Translations)
                .NotNull()
                .Must(ProductTranslationRules.HaveRequiredLanguages)
                .WithMessage(ErrorMessages.Product.TranslationsRequired);

            RuleFor(x => x.Translations)
                .Must(ProductTranslationRules.HaveUniqueLanguageCodes)
                .WithMessage(ErrorMessages.Product.TranslationsUnique);

            RuleForEach(x => x.Translations)
                .SetValidator(new ProductTranslationInputDtoValidator());

            RuleFor(x => x.BrandIds)
                .NotEmpty()
                .WithMessage(ErrorMessages.Product.BrandsRequired)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Product.IdsUnique,
                    ErrorMessages.Fields.Brands));

            RuleFor(x => x.ManufacturerCountryIds)
                .NotEmpty()
                .WithMessage(ErrorMessages.Product.ManufacturerCountriesRequired)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Product.IdsUnique,
                    ErrorMessages.Fields.ManufacturerCountries));

            RuleFor(x => x.ColorIds)
                .NotEmpty()
                .WithMessage(ErrorMessages.Product.ColorsRequired)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Product.IdsUnique,
                    ErrorMessages.Fields.Colors));

            RuleFor(x => x)
                .Must(x => x.Images.Count == x.ImageColorIds.Count)
                .WithMessage(ErrorMessages.Product.ImageColorIdsMismatch);

            RuleFor(x => x)
                .Must(HaveImageForEveryColor)
                .WithMessage(ErrorMessages.Product.ColorImagesRequired);
        }

        private static bool HaveImageForEveryColor(CreateProductDto dto)
        {
            if (dto.ColorIds.Count == 0)
            {
                return false;
            }

            var counts = dto.ImageColorIds
                .GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());

            return dto.ColorIds.All(id => counts.GetValueOrDefault(id) > 0);
        }
    }

    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Size)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Diameter)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.PowerAmperes)
                .GreaterThan(0)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Common.GreaterThan,
                    ErrorMessages.Fields.PowerAmperes,
                    0));

            RuleFor(x => x.CategoryId)
                .GreaterThan(0);

            RuleFor(x => x.SubCategoryId)
                .GreaterThan(0)
                .When(x => x.SubCategoryId.HasValue);

            RuleFor(x => x.Translations)
                .NotNull()
                .Must(ProductTranslationRules.HaveRequiredLanguages)
                .WithMessage(ErrorMessages.Product.TranslationsRequired);

            RuleFor(x => x.Translations)
                .Must(ProductTranslationRules.HaveUniqueLanguageCodes)
                .WithMessage(ErrorMessages.Product.TranslationsUnique);

            RuleForEach(x => x.Translations)
                .SetValidator(new ProductTranslationInputDtoValidator());

            RuleFor(x => x.BrandIds)
                .NotEmpty()
                .WithMessage(ErrorMessages.Product.BrandsRequired)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Product.IdsUnique,
                    ErrorMessages.Fields.Brands));

            RuleFor(x => x.ManufacturerCountryIds)
                .NotEmpty()
                .WithMessage(ErrorMessages.Product.ManufacturerCountriesRequired)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Product.IdsUnique,
                    ErrorMessages.Fields.ManufacturerCountries));

            RuleFor(x => x.ColorIds)
                .NotEmpty()
                .WithMessage(ErrorMessages.Product.ColorsRequired)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage(ErrorMessages.Format(
                    ErrorMessages.Product.IdsUnique,
                    ErrorMessages.Fields.Colors));

            RuleFor(x => x)
                .Must(x => x.Images.Count == x.ImageColorIds.Count)
                .WithMessage(ErrorMessages.Product.ImageColorIdsMismatch);

            // Per-color image presence (keep + new) is validated in the service with DB state.
        }
    }
}
