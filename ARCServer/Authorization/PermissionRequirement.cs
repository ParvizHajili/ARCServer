using Microsoft.AspNetCore.Authorization;

namespace ARCServer.Authorization
{
    public sealed class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permissionCode)
        {
            PermissionCode = permissionCode;
        }

        public string PermissionCode { get; }
    }
}
