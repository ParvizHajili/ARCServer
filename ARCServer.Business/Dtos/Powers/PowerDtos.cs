namespace ARCServer.Business.Dtos.Powers
{
    public class CreatePowerDto
    {
        public decimal Value { get; set; }
    }

    public class UpdatePowerDto
    {
        public decimal Value { get; set; }
    }

    public class PowerDetailDto
    {
        public int Id { get; set; }

        public decimal Value { get; set; }
    }
}
