namespace ARCServer.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public int ProductColorId { get; set; }

        public ProductColor ProductColor { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}
