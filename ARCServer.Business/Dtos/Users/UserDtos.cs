using System.ComponentModel.DataAnnotations;

namespace ARCServer.Business.Dtos.Users
{
    public class UserListItemDto
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public IReadOnlyList<string> Roles { get; set; } = [];

        public int PermissionCount { get; set; }

        public DateTime CreateDate { get; set; }
    }

    public class UserDetailDto
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public IReadOnlyList<string> Roles { get; set; } = [];

        public IReadOnlyList<string> PermissionCodes { get; set; } = [];

        public IReadOnlyList<string> EffectivePermissions { get; set; } = [];

        public DateTime CreateDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public List<string> PermissionCodes { get; set; } = [];
    }

    public class UpdateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public string? Password { get; set; }

        public List<string> PermissionCodes { get; set; } = [];
    }
}
