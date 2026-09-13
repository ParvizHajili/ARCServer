namespace ARCServer.Domain.Entities
{
    public class Color : BaseEntity
    {
        /// <summary>#RRGGBB formatında rəng kodu (dilə bağlı deyil).</summary>
        public string HexCode { get; set; } = string.Empty;

        public ICollection<ColorTranslation> Translations { get; set; } = new List<ColorTranslation>();
    }
}
