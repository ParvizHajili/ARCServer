namespace ARCServer.Domain.Common
{
    /// <summary>
    /// Canonical permission codes used by [RequirePermission] attributes.
    /// Seed sync also discovers these via reflection as a fallback catalog.
    /// </summary>
    public static class PermissionCodes
    {
        public static class Categories
        {
            public const string List = "Categories.List";
            public const string View = "Categories.View";
            public const string Create = "Categories.Create";
            public const string Update = "Categories.Update";
            public const string Delete = "Categories.Delete";
        }

        public static class ManufacturerCountries
        {
            public const string List = "ManufacturerCountries.List";
            public const string View = "ManufacturerCountries.View";
            public const string Create = "ManufacturerCountries.Create";
            public const string Update = "ManufacturerCountries.Update";
            public const string Delete = "ManufacturerCountries.Delete";
        }

        public static class Brands
        {
            public const string List = "Brands.List";
            public const string View = "Brands.View";
            public const string Create = "Brands.Create";
            public const string Update = "Brands.Update";
            public const string Delete = "Brands.Delete";
        }

        public static class Colors
        {
            public const string List = "Colors.List";
            public const string View = "Colors.View";
            public const string Create = "Colors.Create";
            public const string Update = "Colors.Update";
            public const string Delete = "Colors.Delete";
        }

        public static class Products
        {
            public const string List = "Products.List";
            public const string View = "Products.View";
            public const string Create = "Products.Create";
            public const string Update = "Products.Update";
            public const string Delete = "Products.Delete";
        }

        public static class Users
        {
            public const string List = "Users.List";
            public const string View = "Users.View";
            public const string Create = "Users.Create";
            public const string Update = "Users.Update";
            public const string Delete = "Users.Delete";
        }

        public static class Permissions
        {
            public const string List = "Permissions.List";
        }
    }
}
