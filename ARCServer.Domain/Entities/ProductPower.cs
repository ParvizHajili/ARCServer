namespace ARCServer.Domain.Entities
{
    public class ProductPower : BaseEntity
    {
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public int PowerId { get; set; }

        public Power Power { get; set; } = null!;
    }
}
