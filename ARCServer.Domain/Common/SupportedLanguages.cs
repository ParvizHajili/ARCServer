namespace ARCServer.Domain.Common
{
    public static class SupportedLanguages
    {
        /// <summary>
        /// Languages that may be stored. New languages are added here in code only — DB stores LanguageCode as string.
        /// </summary>
        public static readonly string[] AllowedCodes = ["az", "en", "ru"];

        /// <summary>
        /// Required languages for create/update. Only Azerbaijani is mandatory; en/ru are optional.
        /// </summary>
        public static readonly string[] RequiredCodes = ["az"];
    }
}
