using System.Text.RegularExpressions;
using ARCServer.Business.Common.Messages;
using ARCServer.Business.Dtos.Contact;
using FluentValidation;

namespace ARCServer.Business.Validators.Contact
{
    public class CreateContactInquiryDtoValidator : AbstractValidator<CreateContactInquiryDto>
    {
        private static readonly Regex PhoneRegex = new(
            @"^\+994\s(10|50|51|55|60|70|77|99)\s\d{3}-\d{2}-\d{2}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public CreateContactInquiryDtoValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(ErrorMessages.Contact.NameRequired)
                .Length(2, 150)
                .WithMessage(ErrorMessages.Contact.NameLength);

            RuleFor(x => x.Message)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(ErrorMessages.Contact.MessageRequired)
                .Length(4, 2000)
                .WithMessage(ErrorMessages.Contact.MessageLength);

            RuleFor(x => x.Phone)
                .Must(phone => string.IsNullOrWhiteSpace(phone) || PhoneRegex.IsMatch(phone))
                .WithMessage(ErrorMessages.Contact.PhoneInvalid);

            RuleFor(x => x.Email)
                .MaximumLength(200)
                .WithMessage(ErrorMessages.Contact.EmailLength)
                .Must(email => string.IsNullOrWhiteSpace(email) || EmailRegex.IsMatch(email))
                .WithMessage(ErrorMessages.Contact.EmailInvalid);

            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Phone) || !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage(ErrorMessages.Contact.EitherRequired)
                .OverridePropertyName("contact");
        }
    }
}
