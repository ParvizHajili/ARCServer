namespace ARCServer.Business.Dtos.Diameters
{
    public class CreateDiameterDto
    {
        public decimal Value { get; set; }
    }

    public class UpdateDiameterDto
    {
        public decimal Value { get; set; }
    }

    public class DiameterDetailDto
    {
        public int Id { get; set; }

        public decimal Value { get; set; }
    }
}
