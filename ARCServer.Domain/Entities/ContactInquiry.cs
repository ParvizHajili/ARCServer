namespace ARCServer.Domain.Entities
{
    public class ContactInquiry : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
