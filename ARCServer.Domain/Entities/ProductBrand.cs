namespace ARCServer.Domain.Entities
{
    public class ProductBrand : BaseEntity
    {
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public int BrandId { get; set; }

        public Brand Brand { get; set; } = null!;
    }
}
