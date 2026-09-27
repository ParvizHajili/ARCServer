namespace ARCServer.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Code { get; set; } = string.Empty;

        public bool HasWarranty { get; set; }

        public bool IsMadeToOrder { get; set; }

        public int ViewCount { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public int? SubCategoryId { get; set; }

        public SubCategory? SubCategory { get; set; }

        public ICollection<ProductTranslation> Translations { get; set; } = new List<ProductTranslation>();

        public ICollection<ProductBrand> Brands { get; set; } = new List<ProductBrand>();

        public ICollection<ProductSize> Sizes { get; set; } = new List<ProductSize>();

        public ICollection<ProductDiameter> Diameters { get; set; } = new List<ProductDiameter>();

        public ICollection<ProductPower> Powers { get; set; } = new List<ProductPower>();

        public ICollection<ProductManufacturerCountry> ManufacturerCountries { get; set; } =
            new List<ProductManufacturerCountry>();

        public ICollection<ProductColor> Colors { get; set; } = new List<ProductColor>();

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        public ICollection<ProductSpinImage> SpinImages { get; set; } = new List<ProductSpinImage>();
    }
}
