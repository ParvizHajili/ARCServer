using Microsoft.AspNetCore.Identity;

namespace ARCServer.Domain.Entities.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreateDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

        public string DisplayName =>
            string.Join(' ', new[] { FirstName, LastName }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
    }
}
