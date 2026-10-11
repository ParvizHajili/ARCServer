namespace ARCServer.Business.Dtos.Products
{
    public class HomeProductDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }

    public class HomeProductOptionDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsHomePageProduct { get; set; }
    }

    public class SetHomeProductsDto
    {
        public List<int> ProductIds { get; set; } = [];
    }
}
