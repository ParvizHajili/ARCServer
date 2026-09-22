using System.ComponentModel.DataAnnotations;

namespace ARCServer.Business.Dtos.Auth
{
    public class LoginRequestDto
    {
        [Required]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
