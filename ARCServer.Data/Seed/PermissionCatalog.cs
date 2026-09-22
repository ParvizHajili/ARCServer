using System.Reflection;
using ARCServer.Domain.Common;

namespace ARCServer.Data.Seed
{
    /// <summary>
    /// Builds the permission catalog from PermissionCodes nested constants via reflection,
    /// then merges any extra codes discovered from controller [RequirePermission] attributes.
    /// </summary>
    public static class PermissionCatalog
    {
        public static IReadOnlyList<PermissionDefinition> FromPermissionCodes()
        {
            var result = new List<PermissionDefinition>();

            foreach (var moduleType in typeof(PermissionCodes).GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
            {
                foreach (var field in moduleType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                {
                    if (!field.IsLiteral || field.IsInitOnly || field.FieldType != typeof(string))
                    {
                        continue;
                    }

                    var code = (string?)field.GetRawConstantValue();
                    if (string.IsNullOrWhiteSpace(code))
                    {
                        continue;
                    }

                    result.Add(CreateDefinition(code));
                }
            }

            return result
                .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(x => x.Module)
                .ThenBy(x => x.Action)
                .ToList();
        }

        public static IReadOnlyList<PermissionDefinition> Merge(
            IEnumerable<PermissionDefinition> primary,
            IEnumerable<string> extraCodes)
        {
            var map = primary.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

            foreach (var code in extraCodes)
            {
                if (string.IsNullOrWhiteSpace(code) || map.ContainsKey(code))
                {
                    continue;
                }

                map[code] = CreateDefinition(code);
            }

            return map.Values
                .OrderBy(x => x.Module)
                .ThenBy(x => x.Action)
                .ToList();
        }

        public static PermissionDefinition CreateDefinition(string code)
        {
            var parts = code.Split('.', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var module = parts.Length > 0 ? parts[0] : code;
            var action = parts.Length > 1 ? parts[1] : code;

            return new PermissionDefinition
            {
                Code = code,
                Module = module,
                Action = action,
                DisplayName = $"{module} — {action}",
                Description = $"Permission to {action.ToLowerInvariant()} {module.ToLowerInvariant()}.",
            };
        }
    }
}
