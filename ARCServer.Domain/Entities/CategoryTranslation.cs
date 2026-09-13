namespace ARCServer.Domain.Entities
{
    public class CategoryTranslation : BaseEntity
    {
        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        /// <summary>
        /// ISO-like language code (az, en, ru, ...). Dynamic — no schema change for new languages.
        /// </summary>
        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
