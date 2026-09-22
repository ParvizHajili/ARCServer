using ARCServer.Data.Context;
using ARCServer.Domain.Common;
using ARCServer.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.Permissions
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ArcDbContext _db;

        public UserPermissionService(UserManager<ApplicationUser> userManager, ArcDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<bool> HasPermissionAsync(
            int userId,
            string permissionCode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(permissionCode))
            {
                return false;
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null || !user.IsActive)
            {
                return false;
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(AppRoles.SuperAdmin, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            var permissions = await GetEffectivePermissionsAsync(userId, roles, cancellationToken);
            return permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return [];
            }

            var roles = await _userManager.GetRolesAsync(user);
            return await GetEffectivePermissionsAsync(userId, roles, cancellationToken);
        }

        public async Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(
            int userId,
            IList<string> roles,
            CancellationToken cancellationToken = default)
        {
            if (roles.Contains(AppRoles.SuperAdmin, StringComparer.OrdinalIgnoreCase))
            {
                return await _db.Permissions
                    .AsNoTracking()
                    .Select(x => x.Code)
                    .OrderBy(x => x)
                    .ToListAsync(cancellationToken);
            }

            var rolePermissions = await _db.RolePermissions
                .AsNoTracking()
                .Where(rp => _db.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == rp.RoleId))
                .Select(rp => rp.Permission.Code)
                .ToListAsync(cancellationToken);

            var userPermissions = await _db.UserPermissions
                .AsNoTracking()
                .Where(up => up.UserId == userId)
                .Select(up => up.Permission.Code)
                .ToListAsync(cancellationToken);

            return rolePermissions
                .Concat(userPermissions)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }
    }
}
