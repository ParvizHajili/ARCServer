namespace ARCServer.Business.Dtos.Contact
{
    public class CreateContactInquiryDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string Message { get; set; } = string.Empty;
    }

    public class ContactInquiryDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime CreateDate { get; set; }
    }
}
