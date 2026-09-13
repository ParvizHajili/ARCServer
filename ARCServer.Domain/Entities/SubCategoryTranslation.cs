namespace ARCServer.Domain.Entities
{
    public class SubCategoryTranslation : BaseEntity
    {
        public int SubCategoryId { get; set; }

        public SubCategory SubCategory { get; set; } = null!;

        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
