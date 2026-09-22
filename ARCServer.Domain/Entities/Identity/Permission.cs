namespace ARCServer.Domain.Entities.Identity
{
    /// <summary>
    /// Endpoint-level permission discovered from [RequirePermission] attributes.
    /// Code format: Module.Action (e.g. Categories.List).
    /// </summary>
    public class Permission
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}
