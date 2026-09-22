using Microsoft.AspNetCore.Identity;

namespace ARCServer.Domain.Entities.Identity
{
    public class ApplicationRole : IdentityRole<int>
    {
        public string? Description { get; set; }

        /// <summary>
        /// System roles (e.g. SUPERADMIN) cannot be deleted.
        /// </summary>
        public bool IsSystemRole { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
