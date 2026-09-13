namespace ARCServer.Domain.Entities
{
    public class SubCategory : BaseEntity
    {
        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public ICollection<SubCategoryTranslation> Translations { get; set; } = new List<SubCategoryTranslation>();
    }
}
