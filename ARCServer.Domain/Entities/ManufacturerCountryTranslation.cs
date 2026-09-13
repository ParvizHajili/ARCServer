namespace ARCServer.Domain.Entities
{
    public class ManufacturerCountryTranslation : BaseEntity
    {
        public int ManufacturerCountryId { get; set; }

        public ManufacturerCountry ManufacturerCountry { get; set; } = null!;

        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
