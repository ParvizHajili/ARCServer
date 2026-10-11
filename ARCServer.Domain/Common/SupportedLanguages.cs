namespace ARCServer.Domain.Common
{
    public static class SupportedLanguages
    {
        /// <summary>
        /// Fallback language when preferred (en/ru) translation is missing.
        /// </summary>
        public const string DefaultCode = "az";

        /// <summary>
        /// Languages that may be stored. New languages are added here in code only — DB stores LanguageCode as string.
        /// </summary>
        public static readonly string[] AllowedCodes = ["az", "en", "ru"];

        /// <summary>
        /// Required languages for create/update. Only Azerbaijani is mandatory; en/ru are optional.
        /// </summary>
        public static readonly string[] RequiredCodes = [DefaultCode];
    }
}
