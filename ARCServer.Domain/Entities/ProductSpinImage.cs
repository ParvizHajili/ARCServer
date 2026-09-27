namespace ARCServer.Domain.Entities
{
    public class ProductSpinImage : BaseEntity
    {
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}
