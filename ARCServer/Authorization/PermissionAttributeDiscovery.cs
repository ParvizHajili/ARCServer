using System.Reflection;
using ARCServer.Data.Seed;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Authorization
{
    /// <summary>
    /// Discovers permission codes from [RequirePermission] attributes on MVC controllers via reflection.
    /// </summary>
    public static class PermissionAttributeDiscovery
    {
        public static IReadOnlyList<string> DiscoverCodes(Assembly assembly)
        {
            var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var controllerTypes = assembly.GetTypes()
                .Where(t =>
                    t is { IsAbstract: false, IsPublic: true }
                    && typeof(ControllerBase).IsAssignableFrom(t));

            foreach (var controllerType in controllerTypes)
            {
                foreach (var attribute in controllerType.GetCustomAttributes<RequirePermissionAttribute>(inherit: true))
                {
                    if (!string.IsNullOrWhiteSpace(attribute.Permission))
                    {
                        codes.Add(attribute.Permission);
                    }
                }

                foreach (var method in controllerType.GetMethods(
                             BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    foreach (var attribute in method.GetCustomAttributes<RequirePermissionAttribute>(inherit: true))
                    {
                        if (!string.IsNullOrWhiteSpace(attribute.Permission))
                        {
                            codes.Add(attribute.Permission);
                        }
                    }
                }
            }

            return codes.OrderBy(x => x).ToList();
        }

        public static IReadOnlyList<PermissionDefinition> BuildCatalog(Assembly assembly)
        {
            var fromCodes = PermissionCatalog.FromPermissionCodes();
            var fromAttributes = DiscoverCodes(assembly);
            return PermissionCatalog.Merge(fromCodes, fromAttributes);
        }
    }
}
