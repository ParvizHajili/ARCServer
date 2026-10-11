using ARCServer.Domain.Common;

namespace ARCServer.Business.Common.Localization
{
    /// <summary>
    /// Shared translation fallback: preferred language → <see cref="SupportedLanguages.DefaultCode"/> (az) → first available.
    /// </summary>
    public static class TranslationExtensions
    {
        public static string NormalizeLanguageCode(string? languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return SupportedLanguages.DefaultCode;
            }

            var code = languageCode.Trim().ToLowerInvariant();
            return SupportedLanguages.AllowedCodes.Contains(code)
                ? code
                : SupportedLanguages.DefaultCode;
        }

        /// <summary>
        /// Resolves a display name: preferred → az → any non-empty.
        /// </summary>
        public static string ResolveTranslatedName(
            this IEnumerable<(string LanguageCode, string Name)> translations,
            string? preferredLanguage = null)
        {
            var preferred = NormalizeLanguageCode(preferredLanguage);
            var list = translations
                .Where(t => !string.IsNullOrWhiteSpace(t.Name))
                .Select(t => (
                    LanguageCode: (t.LanguageCode ?? string.Empty).Trim().ToLowerInvariant(),
                    Name: t.Name.Trim()))
                .Where(t => t.LanguageCode.Length > 0)
                .ToList();

            if (list.Count == 0)
            {
                return string.Empty;
            }

            return FindName(list, preferred)
                ?? FindName(list, SupportedLanguages.DefaultCode)
                ?? list[0].Name;
        }

        /// <summary>
        /// Same as <see cref="ResolveTranslatedName(IEnumerable{ValueTuple{string,string}},string?)"/> but skips soft-deleted rows.
        /// </summary>
        public static string ResolveTranslatedName(
            this IEnumerable<(string LanguageCode, string Name, int Deleted)> translations,
            string? preferredLanguage = null) =>
            translations
                .Where(t => t.Deleted == 0)
                .Select(t => (t.LanguageCode, t.Name))
                .ResolveTranslatedName(preferredLanguage);

        /// <summary>
        /// Picks the best translation entity: preferred → az → first active.
        /// </summary>
        public static T? ResolveTranslation<T>(
            this IEnumerable<T> translations,
            Func<T, string> languageCodeSelector,
            Func<T, int>? deletedSelector = null,
            string? preferredLanguage = null)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(languageCodeSelector);

            var preferred = NormalizeLanguageCode(preferredLanguage);
            var active = translations
                .Where(t => deletedSelector is null || deletedSelector(t) == 0)
                .ToList();

            if (active.Count == 0)
            {
                return null;
            }

            return FindEntity(active, languageCodeSelector, preferred)
                ?? FindEntity(active, languageCodeSelector, SupportedLanguages.DefaultCode)
                ?? active[0];
        }

        private static string? FindName(
            IReadOnlyList<(string LanguageCode, string Name)> list,
            string languageCode)
        {
            foreach (var item in list)
            {
                if (item.LanguageCode == languageCode)
                {
                    return item.Name;
                }
            }

            return null;
        }

        private static T? FindEntity<T>(
            IReadOnlyList<T> list,
            Func<T, string> languageCodeSelector,
            string languageCode)
            where T : class
        {
            foreach (var item in list)
            {
                var code = languageCodeSelector(item)?.Trim();
                if (string.Equals(code, languageCode, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return null;
        }
    }
}
