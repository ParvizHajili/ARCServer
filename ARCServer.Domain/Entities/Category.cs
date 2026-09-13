namespace ARCServer.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Image { get; set; } = string.Empty;

        public int Order { get; set; }

        public ICollection<CategoryTranslation> Translations { get; set; } = new List<CategoryTranslation>();

        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }
}
