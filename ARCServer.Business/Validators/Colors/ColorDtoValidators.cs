using System.Text.RegularExpressions;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Common.Validation;
using ARCServer.Business.Dtos.Colors;
using ARCServer.Business.Validators.Categories;
using FluentValidation;

namespace ARCServer.Business.Validators.Colors
{
    public static class ColorHexRules
    {
        private static readonly Regex HexRegex = new(
            @"^#[0-9A-Fa-f]{6}$",
            RegexOptions.Compiled);

        public static bool IsValid(string? hex) =>
            !string.IsNullOrWhiteSpace(hex) && HexRegex.IsMatch(hex.Trim());

        public static string Normalize(string hex) => hex.Trim().ToUpperInvariant();
    }

    public class CreateColorDtoValidator : AbstractValidator<CreateColorDto>
    {
        public CreateColorDtoValidator()
        {
            RuleFor(x => x.HexCode)
                .NotEmpty()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.HexCode))
                .Must(ColorHexRules.IsValid)
                .WithMessage(ErrorMessages.Color.InvalidHex);

            RuleFor(x => x.Translations)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.Translations))
                .Must(TranslationRules.HaveRequiredLanguages)
                .WithMessage(ErrorMessages.Color.TranslationsRequired);

            RuleFor(x => x.Translations)
                .Must(TranslationRules.HaveUniqueLanguageCodes)
                .WithMessage(ErrorMessages.Color.TranslationsUnique);

            RuleForEach(x => x.Translations)
                .SetValidator(new TranslationInputDtoValidator());
        }
    }

    public class UpdateColorDtoValidator : AbstractValidator<UpdateColorDto>
    {
        public UpdateColorDtoValidator()
        {
            RuleFor(x => x.HexCode)
                .NotEmpty()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.HexCode))
                .Must(ColorHexRules.IsValid)
                .WithMessage(ErrorMessages.Color.InvalidHex);

            RuleFor(x => x.Translations)
                .NotNull()
                .WithMessage(ErrorMessages.Format(ErrorMessages.Common.Required, ErrorMessages.Fields.Translations))
                .Must(TranslationRules.HaveRequiredLanguages)
                .WithMessage(ErrorMessages.Color.TranslationsRequired);

            RuleFor(x => x.Translations)
                .Must(TranslationRules.HaveUniqueLanguageCodes)
                .WithMessage(ErrorMessages.Color.TranslationsUnique);

            RuleForEach(x => x.Translations)
                .SetValidator(new TranslationInputDtoValidator());
        }
    }
}
