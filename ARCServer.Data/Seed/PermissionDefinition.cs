namespace ARCServer.Data.Seed
{
    public sealed class PermissionDefinition
    {
        public required string Code { get; init; }

        public required string Module { get; init; }

        public required string Action { get; init; }

        public required string DisplayName { get; init; }

        public string? Description { get; init; }
    }
}
