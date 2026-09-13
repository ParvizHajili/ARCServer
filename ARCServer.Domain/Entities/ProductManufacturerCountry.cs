namespace ARCServer.Domain.Entities
{
    public class ProductManufacturerCountry : BaseEntity
    {
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public int ManufacturerCountryId { get; set; }

        public ManufacturerCountry ManufacturerCountry { get; set; } = null!;
    }
}
