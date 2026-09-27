namespace ARCServer.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        /// <summary>
        /// Null when the image belongs to the product gallery and is not tied to a color.
        /// </summary>
        public int? ProductColorId { get; set; }

        public ProductColor? ProductColor { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}
