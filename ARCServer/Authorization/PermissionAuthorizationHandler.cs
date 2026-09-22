using System.Security.Claims;
using ARCServer.Business.Services.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace ARCServer.Authorization
{
    /// <summary>
    /// Checks permissions from the database at request time (not only JWT claims),
    /// so permission changes apply without re-login.
    /// </summary>
    public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserPermissionService _permissionService;

        public PermissionAuthorizationHandler(IUserPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var user = context.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub");

            if (!int.TryParse(userIdValue, out var userId))
            {
                return;
            }

            var allowed = await _permissionService.HasPermissionAsync(
                userId,
                requirement.PermissionCode);

            if (allowed)
            {
                context.Succeed(requirement);
            }
        }
    }
}
