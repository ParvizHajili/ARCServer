namespace ARCServer.Business.Dtos.Products
{
    public class ProductSpinImageDto
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int Order { get; set; }
    }

    public class ProductSpinDto
    {
        public int ProductId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public List<ProductSpinImageDto> Images { get; set; } = [];
    }
}
