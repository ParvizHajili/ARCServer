namespace ARCServer.Domain.Entities
{
    public class Product360Video : BaseEntity
    {
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public string VideoUrl { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}
