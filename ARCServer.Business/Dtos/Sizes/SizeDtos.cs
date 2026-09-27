namespace ARCServer.Business.Dtos.Sizes
{
    public class CreateSizeDto
    {
        public decimal Value { get; set; }
    }

    public class UpdateSizeDto
    {
        public decimal Value { get; set; }
    }

    public class SizeDetailDto
    {
        public int Id { get; set; }

        public decimal Value { get; set; }
    }
}
