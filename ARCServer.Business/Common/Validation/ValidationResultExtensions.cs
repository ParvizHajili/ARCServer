using FluentValidation.Results;

namespace ARCServer.Business.Common.Validation
{
    public static class ValidationResultExtensions
    {
        public static Dictionary<string, string[]> ToErrorDictionary(this ValidationResult result)
        {
            return result.Errors
                .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "request" : e.PropertyName)
                .ToDictionary(
                    g => ToCamelCase(g.Key),
                    g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
        }

        private static string ToCamelCase(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || char.IsLower(propertyName[0]))
            {
                return propertyName;
            }

            // Keep nested paths like SubCategories[0].Translations
            var parts = propertyName.Split('.');
            parts[0] = char.ToLowerInvariant(parts[0][0]) + parts[0][1..];
            return string.Join('.', parts);
        }
    }
}
