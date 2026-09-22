using Microsoft.AspNetCore.Authorization;

namespace ARCServer.Authorization
{
    /// <summary>
    /// Marks an endpoint with a permission code (e.g. Categories.List).
    /// Creates a dynamic authorization policy via PermissionPolicyProvider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class RequirePermissionAttribute : AuthorizeAttribute
    {
        public const string PolicyPrefix = "Permission:";

        public string Permission { get; }

        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
            Policy = $"{PolicyPrefix}{permission}";
        }
    }
}
