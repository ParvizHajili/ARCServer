namespace ARCServer.Business.Services.Permissions
{
    public interface IUserPermissionService
    {
        Task<bool> HasPermissionAsync(
            int userId,
            string permissionCode,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(
            int userId,
            IList<string> roles,
            CancellationToken cancellationToken = default);
    }
}
