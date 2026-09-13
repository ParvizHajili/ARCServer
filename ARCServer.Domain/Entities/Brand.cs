namespace ARCServer.Domain.Entities
{
    public class Brand : BaseEntity
    {
        public ICollection<BrandTranslation> Translations { get; set; } = new List<BrandTranslation>();
    }
}
