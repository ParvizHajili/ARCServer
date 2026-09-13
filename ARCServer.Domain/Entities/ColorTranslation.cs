namespace ARCServer.Domain.Entities
{
    public class ColorTranslation : BaseEntity
    {
        public int ColorId { get; set; }

        public Color Color { get; set; } = null!;

        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
