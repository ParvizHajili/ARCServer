namespace ARCServer.Business.Dtos.Permissions
{
    public class PermissionItemDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class PermissionModuleDto
    {
        public string Module { get; set; } = string.Empty;

        public IReadOnlyList<PermissionItemDto> Permissions { get; set; } = [];
    }
}
