using ARCServer.Data.Context;
using ARCServer.Domain.Common;
using ARCServer.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ARCServer.Data.Seed
{
    public static class IdentitySeeder
    {
        public const string DefaultAdminUserName = "admin";
        public const string DefaultAdminEmail = "admin@arc.local";
        public const string DefaultAdminPassword = "Admin123";
        public const string DefaultAdminFirstName = "ARC";
        public const string DefaultAdminLastName = "Super Admin";

        public static async Task SeedAsync(
            IServiceProvider services,
            IReadOnlyList<PermissionDefinition> permissions,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            var db = services.GetRequiredService<ArcDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            await SyncPermissionsAsync(db, permissions, logger, cancellationToken);
            await EnsureSuperAdminRoleAsync(roleManager, logger);
            await AssignAllPermissionsToSuperAdminAsync(db, logger, cancellationToken);
            await EnsureSuperAdminUserAsync(userManager, logger);
        }

        private static async Task SyncPermissionsAsync(
            ArcDbContext db,
            IReadOnlyList<PermissionDefinition> permissions,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if (permissions.Count == 0)
            {
                logger.LogWarning("Permission catalog is empty — nothing to sync.");
                return;
            }

            var existing = await db.Permissions
                .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

            var added = 0;
            foreach (var definition in permissions)
            {
                if (existing.TryGetValue(definition.Code, out var permission))
                {
                    permission.Module = definition.Module;
                    permission.Action = definition.Action;
                    permission.DisplayName = definition.DisplayName;
                    permission.Description = definition.Description;
                    continue;
                }

                db.Permissions.Add(new Permission
                {
                    Code = definition.Code,
                    Module = definition.Module,
                    Action = definition.Action,
                    DisplayName = definition.DisplayName,
                    Description = definition.Description,
                });
                added++;
            }

            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Permission catalog synced. Total definitions: {Total}, newly added: {Added}.",
                permissions.Count,
                added);
        }

        private static async Task EnsureSuperAdminRoleAsync(
            RoleManager<ApplicationRole> roleManager,
            ILogger logger)
        {
            var existing = await roleManager.FindByNameAsync(AppRoles.SuperAdmin);
            if (existing is not null)
            {
                if (!existing.IsSystemRole)
                {
                    existing.IsSystemRole = true;
                    existing.Description ??= "Full system access";
                    await roleManager.UpdateAsync(existing);
                }

                return;
            }

            var role = new ApplicationRole
            {
                Name = AppRoles.SuperAdmin,
                NormalizedName = AppRoles.SuperAdmin.ToUpperInvariant(),
                Description = "Full system access",
                IsSystemRole = true,
            };

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create SUPERADMIN role: {FormatErrors(result)}");
            }

            logger.LogInformation("SUPERADMIN role created.");
        }

        private static async Task AssignAllPermissionsToSuperAdminAsync(
            ArcDbContext db,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var role = await db.Roles
                .FirstOrDefaultAsync(x => x.Name == AppRoles.SuperAdmin, cancellationToken);

            if (role is null)
            {
                throw new InvalidOperationException("SUPERADMIN role was not found after seed.");
            }

            var permissionIds = await db.Permissions
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var existingPermissionIds = await db.RolePermissions
                .Where(x => x.RoleId == role.Id)
                .Select(x => x.PermissionId)
                .ToListAsync(cancellationToken);

            var existingSet = existingPermissionIds.ToHashSet();
            var missing = permissionIds.Where(id => !existingSet.Contains(id)).ToList();

            if (missing.Count == 0)
            {
                logger.LogInformation("SUPERADMIN already has all permissions.");
                return;
            }

            foreach (var permissionId in missing)
            {
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId,
                });
            }

            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Assigned {Count} permission(s) to SUPERADMIN.",
                missing.Count);
        }

        private static async Task EnsureSuperAdminUserAsync(
            UserManager<ApplicationUser> userManager,
            ILogger logger)
        {
            var user = await userManager.FindByNameAsync(DefaultAdminUserName)
                ?? await userManager.FindByEmailAsync(DefaultAdminEmail);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = DefaultAdminUserName,
                    Email = DefaultAdminEmail,
                    EmailConfirmed = true,
                    FirstName = DefaultAdminFirstName,
                    LastName = DefaultAdminLastName,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow,
                };

                var createResult = await userManager.CreateAsync(user, DefaultAdminPassword);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create admin user: {FormatErrors(createResult)}");
                }

                logger.LogInformation(
                    "Admin user '{UserName}' created with default password.",
                    DefaultAdminUserName);
            }
            else
            {
                var changed = false;

                if (!user.IsActive)
                {
                    user.IsActive = true;
                    changed = true;
                }

                if (string.IsNullOrWhiteSpace(user.FirstName))
                {
                    user.FirstName = DefaultAdminFirstName;
                    changed = true;
                }

                if (string.IsNullOrWhiteSpace(user.LastName))
                {
                    user.LastName = DefaultAdminLastName;
                    changed = true;
                }

                if (changed)
                {
                    user.UpdatedDate = DateTime.UtcNow;
                    await userManager.UpdateAsync(user);
                }
            }

            if (!await userManager.IsInRoleAsync(user, AppRoles.SuperAdmin))
            {
                var roleResult = await userManager.AddToRoleAsync(user, AppRoles.SuperAdmin);
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to assign SUPERADMIN role: {FormatErrors(roleResult)}");
                }

                logger.LogInformation("Assigned SUPERADMIN role to '{UserName}'.", user.UserName);
            }
        }

        private static string FormatErrors(IdentityResult result)
        {
            return string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
        }
    }
}
