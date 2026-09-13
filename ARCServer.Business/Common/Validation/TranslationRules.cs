using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Categories;
using ARCServer.Domain.Common;

namespace ARCServer.Business.Common.Validation
{
    public static class TranslationRules
    {
        public static bool IsAllowedLanguage(string? languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return false;
            }

            var code = languageCode.Trim().ToLowerInvariant();
            return SupportedLanguages.AllowedCodes.Contains(code);
        }

        public static bool HaveRequiredLanguages(List<TranslationInputDto> translations)
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

        public static bool HaveUniqueLanguageCodes(List<TranslationInputDto> translations)
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
}
