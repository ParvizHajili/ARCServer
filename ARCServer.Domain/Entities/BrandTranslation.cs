namespace ARCServer.Domain.Entities
{
    public class BrandTranslation : BaseEntity
    {
        public int BrandId { get; set; }

        public Brand Brand { get; set; } = null!;

        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
