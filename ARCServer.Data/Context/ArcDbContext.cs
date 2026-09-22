using System.Linq.Expressions;
using ARCServer.Domain.Entities;
using ARCServer.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Data.Context
{
    public class ArcDbContext
        : IdentityDbContext<
            ApplicationUser,
            ApplicationRole,
            int,
            IdentityUserClaim<int>,
            IdentityUserRole<int>,
            IdentityUserLogin<int>,
            IdentityRoleClaim<int>,
            IdentityUserToken<int>>
    {
        public ArcDbContext(DbContextOptions<ArcDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();

        public DbSet<CategoryTranslation> CategoryTranslations => Set<CategoryTranslation>();

        public DbSet<SubCategory> SubCategories => Set<SubCategory>();

        public DbSet<SubCategoryTranslation> SubCategoryTranslations => Set<SubCategoryTranslation>();

        public DbSet<ManufacturerCountry> ManufacturerCountries => Set<ManufacturerCountry>();

        public DbSet<ManufacturerCountryTranslation> ManufacturerCountryTranslations =>
            Set<ManufacturerCountryTranslation>();

        public DbSet<Brand> Brands => Set<Brand>();

        public DbSet<BrandTranslation> BrandTranslations => Set<BrandTranslation>();

        public DbSet<Color> Colors => Set<Color>();

        public DbSet<ColorTranslation> ColorTranslations => Set<ColorTranslation>();

        public DbSet<Product> Products => Set<Product>();

        public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();

        public DbSet<ProductBrand> ProductBrands => Set<ProductBrand>();

        public DbSet<ProductManufacturerCountry> ProductManufacturerCountries =>
            Set<ProductManufacturerCountry>();

        public DbSet<ProductColor> ProductColors => Set<ProductColor>();

        public DbSet<ProductImage> ProductImages => Set<ProductImage>();

        public DbSet<Permission> Permissions => Set<Permission>();

        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        public DbSet<UserPermission> UserPermissions => Set<UserPermission>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArcDbContext).Assembly);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    continue;
                }

                ConfigureBaseEntity(modelBuilder, entityType.ClrType);
            }
        }

        private static void ConfigureBaseEntity(ModelBuilder modelBuilder, Type entityType)
        {
            modelBuilder.Entity(entityType, builder =>
            {
                builder.HasKey(nameof(BaseEntity.Id));

                builder.Property(nameof(BaseEntity.Id))
                    .ValueGeneratedOnAdd();

                builder.Property(nameof(BaseEntity.Deleted))
                    .HasDefaultValue(0)
                    .IsRequired();

                builder.Property(nameof(BaseEntity.CreateDate))
                    .IsRequired();

                builder.HasIndex(nameof(BaseEntity.Id), nameof(BaseEntity.Deleted))
                    .IsUnique();
            });

            var parameter = Expression.Parameter(entityType, "e");
            var deletedProperty = Expression.Property(parameter, nameof(BaseEntity.Deleted));
            var filter = Expression.Lambda(
                Expression.Equal(deletedProperty, Expression.Constant(0)),
                parameter);

            modelBuilder.Entity(entityType).HasQueryFilter(filter);
        }
    }
}
