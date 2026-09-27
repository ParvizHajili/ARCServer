namespace ARCServer.Domain.Entities
{
    public class ProductDiameter : BaseEntity
    {
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public int DiameterId { get; set; }

        public Diameter Diameter { get; set; } = null!;
    }
}
