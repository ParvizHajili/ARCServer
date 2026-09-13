namespace ARCServer.Domain.Entities
{
    public class ManufacturerCountry : BaseEntity
    {
        public ICollection<ManufacturerCountryTranslation> Translations { get; set; } =
            new List<ManufacturerCountryTranslation>();
    }
}
